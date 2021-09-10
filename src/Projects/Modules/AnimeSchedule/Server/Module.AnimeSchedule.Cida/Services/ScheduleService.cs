using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;
using Module.AnimeSchedule.Cida.Services.Source;
using NLog;

namespace Module.AnimeSchedule.Cida.Services
{
    public class ScheduleService
    {
        private readonly Func<AnimeScheduleDbContext> getContext;
        private readonly ILogger logger;
        private List<IActionService> actionServices;
        private List<(Schedule schedule, Task task)> schedules = new List<(Schedule, Task)>();

        public IReadOnlyList<Schedule> Schedules => this.schedules.Select(x => x.schedule).ToList().AsReadOnly();

        public ScheduleService(IEnumerable<IActionService> actionServices, Func<AnimeScheduleDbContext> getContext, ILogger logger)
        {
            this.actionServices = new List<IActionService>(actionServices);
            this.getContext = getContext;
            this.logger = logger;
        }

        public async Task Initialize(CancellationToken cancellationToken, CrunchyrollSourceService crunchyrollSourceService, NiblSourceService niblSourceService)
        {
            using var context = this.getContext();

            var schedules = await EntityFrameworkQueryableExtensions.ToArrayAsync(context.Schedules.Include(x => x.AnimeContexts).ThenInclude(x => x.Episodes).ThenInclude(x => x.PackageNumber), cancellationToken);
            foreach (var schedule in schedules)
            {
                this.StartSchedule(Models.Schedule.Schedule.FromDb(schedule, crunchyrollSourceService, niblSourceService, new CancellationTokenSource()));
            }
        }

        public void StartSchedule(Schedule schedule)
        {
            var scheduleTask = new Task(async () => await this.Schedule(schedule, default), schedule.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            scheduleTask.Start();
            this.schedules.Add((schedule, scheduleTask));
        }

        private async Task Schedule(Schedule schedule, CancellationToken cancellationToken)
        {
            this.logger.Info($"Starting Schedule '{schedule.Name}'");
            while (true)
            {
                schedule.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                if (DateTime.Now > schedule.StartDate)
                {
                    this.logger.Info($"Run Schedule '{schedule.Name}'");

                    foreach (var item in schedule.Animes)
                    {
                        var newEpisodes = await item.NewEpisodesAvailable(cancellationToken);

                        foreach (var episode in newEpisodes)
                        {
                            foreach (var actionService in this.actionServices)
                            {
                                await actionService.Execute(item, episode, schedule.CancellationTokenSource.Token);
                            }
                        }
                    }
                }

                await Task.Delay(schedule.Interval);
            }
        }
    }
}
