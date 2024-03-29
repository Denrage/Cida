﻿using Cida.Api;
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;
using Module.AnimeSchedule.Cida.Services.Source;
using NLog;

namespace Module.AnimeSchedule.Cida.Services;

public class ScheduleService
{
    private readonly Func<AnimeScheduleDbContext> getContext;
    private readonly IModuleLogger moduleLogger;
    private readonly ILogger logger;
    private readonly Dictionary<AnimeInfoType, AnimeInfoHandlerBase> handlers;
    private readonly List<IActionService> actionServices;
    private readonly List<IMultiActionService> multiActionServices;
    private readonly Dictionary<int, ScheduleContext> schedules = new();
    //private readonly List<(int scheduleId, Task task)> schedules = new();
    private readonly SemaphoreSlim scheduleSemaphore = new(1);

    //public IReadOnlyList<Schedule> Schedules => this.schedules.Select(x => x.schedule).ToList().AsReadOnly();

    public ScheduleService(IEnumerable<IActionService> actionServices, IEnumerable<IMultiActionService> multiActionServices, Func<AnimeScheduleDbContext> getContext, IModuleLogger moduleLogger, ILogger logger)
    {
        this.actionServices = new List<IActionService>(actionServices);
        this.multiActionServices = new List<IMultiActionService>(multiActionServices);
        this.getContext = getContext;
        this.logger = logger;
        this.moduleLogger = moduleLogger;
        var anilistClient = new Anilist4Net.Client();
        this.handlers = new Dictionary<AnimeInfoType, AnimeInfoHandlerBase>()
        {
            { AnimeInfoType.Crunchyroll, new CrunchyrollAnimeInfoHandler(this.moduleLogger.CreateSubLogger("Crunchyroll-Source"), anilistClient, this.getContext) },
            { AnimeInfoType.Nibl, new NiblAnimeInfoHandler(this.moduleLogger.CreateSubLogger("Nibl-Source"), anilistClient, this.getContext) },
        };
    }

    public async Task Initialize(CancellationToken cancellationToken)
    {
        using var context = this.getContext();

        var schedules = await AsyncEnumerable.Select(context.Schedules, x => x.Id).ToArrayAsync(cancellationToken);
        foreach (var schedule in schedules)
        {
            await this.StartSchedule(schedule);
        }
    }

    public async Task<bool> StartSchedule(int scheduleId)
    {
        ScheduleContext initializeContext(ScheduleContext context)
        {
            context.ScheduleId = scheduleId;
            context.CancellationTokenSource = new CancellationTokenSource();
            context.ForceRunTokenSource = new CancellationTokenSource();
            context.State = ScheduleState.Running;
            var scheduleTask = new Task(() => this.Schedule(context).Wait(), context.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            scheduleTask.Start();
            context.ScheduleTask = scheduleTask;
            return context;
        }

        if (this.schedules.TryGetValue(scheduleId, out var existingSchedule))
        {
            if (existingSchedule.ScheduleTask.Status == TaskStatus.Running)
            {
                return false;
            }

            initializeContext(existingSchedule);

            return true;
        }

        using var context = this.getContext();
        var dbSchedule = await context.Schedules.AsQueryable().FirstOrDefaultAsync(x => x.Id == scheduleId);
        if (dbSchedule != null)
        {
            if (dbSchedule.StartDate.Equals(DateTime.MaxValue))
            {
                this.logger.Info($"Skipping schedule {dbSchedule.Id} - {dbSchedule.Name} bc it is deactivated");
                return false;
            }
            this.schedules.Add(scheduleId, initializeContext(new ScheduleContext()));
            return true;
        }

        this.logger.Warn($"Schedule with id '{scheduleId}' not found! This schedule will be ignored and wont be started!");
        return false;
    }

    public bool StopSchedule(int scheduleId)
    {
        if (this.schedules.TryGetValue(scheduleId, out var scheduleContext))
        {
            scheduleContext.CancellationTokenSource.Cancel();
            scheduleContext.State = ScheduleState.Stopped;
            return true;
        }
        return false;
    }

    public bool ForceRunSchedule(int scheduleId)
    {
        if (this.schedules.TryGetValue(scheduleId, out var scheduleContext))
        {
            scheduleContext.ForceRunTokenSource.Cancel();
            return true;
        }

        return false;
    }

    public ScheduleState? GetScheduleState(int scheduleId)
    {
        if (this.schedules.TryGetValue(scheduleId, out var scheduleContext))
        {
            return scheduleContext.State;
        }

        return null;
    }

    private async Task Schedule(ScheduleContext scheduleContext)
    {
        try
        {
            var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(scheduleContext.CancellationTokenSource.Token, scheduleContext.ForceRunTokenSource.Token);
            while (true)
            {
                var context = this.getContext();
                var schedule = await context.Schedules.Include(x => x.Animes).FirstOrDefaultAsync(x => x.Id == scheduleContext.ScheduleId, scheduleContext.CancellationTokenSource.Token);
                context.Dispose();

                scheduleContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                if (schedule.StartDate.Equals(DateTime.MaxValue))
                {
                    this.logger.Info($"Stopping schedule {schedule.Id} - {schedule.Name} bc it was deactivated");
                    return;
                }

                if (DateTime.Now > schedule.StartDate)
                {
                    scheduleContext.State = ScheduleState.Running;
                    this.logger.Info($"Run Schedule '{schedule.Name}'");

                    await this.scheduleSemaphore.WaitAsync(scheduleContext.CancellationTokenSource.Token);
                    try
                    {

                        foreach (var item in schedule.Animes)
                        {
                            var handler = this.GetHandler(item);
                            this.logger.Info($"Checking for new episodes for '{item.Identifier}'");
                            var newEpisodes = await handler.GetNewEpisodes(item, scheduleContext.ScheduleId, scheduleContext.CancellationTokenSource.Token);

                            foreach (var episode in newEpisodes)
                            {
                                foreach (var actionService in this.actionServices)
                                {
                                    await actionService.Execute(episode, scheduleContext.ScheduleId, scheduleContext.CancellationTokenSource.Token);
                                }
                            }

                            foreach (var multiActionService in this.multiActionServices)
                            {
                                await multiActionService.Execute(newEpisodes, scheduleContext.ScheduleId, scheduleContext.CancellationTokenSource.Token);
                            }
                        }
                        this.logger.Info($"Finished Schedule '{schedule.Name}'");
                    }
                    finally
                    {
                        this.scheduleSemaphore.Release();
                    }
                }

                scheduleContext.State = ScheduleState.Waiting;
                try
                {
                    await Task.Delay(schedule.Interval, combinedCancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    if (scheduleContext.CancellationTokenSource.IsCancellationRequested)
                    {
                        throw;
                    }
                    else
                    {
                        scheduleContext.ForceRunTokenSource = new CancellationTokenSource();
                        combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(scheduleContext.CancellationTokenSource.Token, scheduleContext.ForceRunTokenSource.Token);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore this bc, ScheduleTasks are cancelable.
        }
        catch (Exception ex)
        {
            this.logger.Error(ex, $"An exception occured during execution of schedule {scheduleContext.ScheduleId}. Schedule will be stopped.");
        }
    }

    public async Task<IEnumerable<AnimeTestResult>> TestAnime(AnimeInfo info, CancellationToken cancellationToken)
    {
        var handler = this.GetHandler(info);
        var testResults = await handler.GetTestResults(info, cancellationToken);
        return testResults.Select(x => x.GetTestResult());
    }

    private AnimeInfoHandlerBase GetHandler(AnimeInfo animeInfo)
    {
        if (this.handlers.TryGetValue(animeInfo.Type, out var handler))
        {
            return handler;
        }

        throw new InvalidOperationException($"There is no '{nameof(AnimeInfoHandlerBase)}' for '{animeInfo.Type}'!");
    }
}
