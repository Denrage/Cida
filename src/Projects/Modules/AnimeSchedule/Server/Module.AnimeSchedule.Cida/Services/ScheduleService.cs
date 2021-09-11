using Cida.Api;
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
    private readonly List<(int scheduleId, Task task)> schedules = new();
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
            this.StartSchedule(schedule);
        }
    }

    public async void StartSchedule(int scheduleId)
    {
        using var context = this.getContext();

        if (await EntityFrameworkQueryableExtensions.AnyAsync(context.Schedules, x => x.Id == scheduleId))
        {
            var scheduleContext = new ScheduleContext()
            {
                CancellationTokenSource = new CancellationTokenSource(),
                ScheduleId = scheduleId,
            };
            var scheduleTask = new Task(async () => await this.Schedule(scheduleContext), scheduleContext.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            scheduleTask.Start();
            this.schedules.Add((scheduleId, scheduleTask));
        }
        else
        {
            this.logger.Warn($"Schedule with id '{scheduleId}' not found! This schedule will be ignored and wont be started!");
        }

    }

    private async Task Schedule(ScheduleContext scheduleContext)
    {
        while (true)
        {
            var context = this.getContext();
            var schedule = await context.Schedules.Include(x => x.Animes).FirstOrDefaultAsync(x => x.Id == scheduleContext.ScheduleId, scheduleContext.CancellationTokenSource.Token);
            context.Dispose();

            scheduleContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (DateTime.Now > schedule.StartDate)
            {
                this.logger.Info($"Run Schedule '{schedule.Name}'");

                await this.scheduleSemaphore.WaitAsync();
                try
                {

                    foreach (var item in schedule.Animes)
                    {
                        var handler = this.GetHandler(item);
                        this.logger.Info($"Checking for new episodes for '{item.Identifier}'");
                        var newEpisodes = await handler.GetNewEpisodes(item, scheduleContext.CancellationTokenSource.Token);

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
                }
                finally
                {
                    this.scheduleSemaphore.Release();
                }
            }

            await Task.Delay(schedule.Interval);
        }
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
