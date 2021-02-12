using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
                this.StartSchedule(Cida.Services.Schedule.FromDb(schedule, crunchyrollSourceService, niblSourceService, new CancellationTokenSource()));
            }
        }

        public void StartSchedule(Schedule schedule)
        {
            var scheduleTask = new Task(async () => await this.Schedule(schedule), schedule.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            scheduleTask.Start();
            this.schedules.Add((schedule, scheduleTask));
        }

        private async Task Schedule(Schedule schedule)
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
                        var newEpisodes = await item.NewEpisodesAvailable();

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

    public class Schedule
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public List<AnimeInfoContext> Animes { get; set; }

        public TimeSpan Interval { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        // TODO: put that rather into a factory
        public static Schedule FromDb(Models.Database.Schedule schedule, CrunchyrollSourceService crunchyrollSourceService, NiblSourceService niblSourceService, CancellationTokenSource cancellationTokenSource)
        {
            return new Schedule()
            {
                Animes = schedule.AnimeContexts.Select(x => AnimeInfoContext.FromDb(x, crunchyrollSourceService, niblSourceService)).ToList(),
                CancellationTokenSource = cancellationTokenSource,
                Interval = schedule.Interval,
                StartDate = schedule.StartDate,
                Name = schedule.Name,
            };
        }
    }

    public abstract class AnimeInfoContext
    {
        protected ISourceService SourceService { get; }

        public string Identifier { get; set; }

        public ulong MyAnimeListId { get; set; }

        public string Filter { get; set; }

        public List<IAnimeInfo> Episodes { get; set; } = new List<IAnimeInfo>();

        protected AnimeInfoContext(ISourceService sourceService)
        {
            this.SourceService = sourceService;
        }

        public abstract Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable();

        // TODO: put that rather into a factory
        public static AnimeInfoContext FromDb(Models.Database.AnimeContext context, CrunchyrollSourceService crunchyrollSourceService, NiblSourceService niblSourceService)
        {
            AnimeInfoContext result = null;
            switch (context.Type)
            {
                case Models.Database.AnimeContextType.Crunchyroll:
                    result = new CrunchyrollAnimeInfoContext(crunchyrollSourceService)
                    {
                        Episodes = context.Episodes.Select(x => AnimeInfo.FromDb(x)).ToList(),
                        Filter = context.Filter,
                        Identifier = context.Identifier,
                        MyAnimeListId = context.MyAnimeListId,
                    };
                    break;

                case Models.Database.AnimeContextType.Nibl:
                    result = new NiblAnimeInfoContext(niblSourceService)
                    {
                        Episodes = context.Episodes.Select(x => AnimeInfo.FromDb(x)).ToList(),
                        Filter = context.Filter,
                        Identifier = context.Identifier,
                        MyAnimeListId = context.MyAnimeListId,
                        FolderName = context.FolderName,
                    };
                    break;

                default:
                    break;
            }

            return result;
        }
    }

    public abstract class AnimeInfoContext<TSourceService> : AnimeInfoContext
       where TSourceService : ISourceService
    {
        protected AnimeInfoContext(ISourceService sourceService)
            : base(sourceService)
        {
        }

        protected new TSourceService SourceService => (TSourceService)base.SourceService;
    }

    public class CrunchyrollAnimeInfoContext : AnimeInfoContext<CrunchyrollSourceService>
    {
        public CrunchyrollAnimeInfoContext(ISourceService sourceService)
            : base(sourceService)
        {
        }

        public override async Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable()
        {
            var newEpisodes = await this.SourceService.GetNewEpisodes(this);
            this.Episodes.AddRange(newEpisodes);
            return newEpisodes;
        }
    }

    public class NiblAnimeInfoContext : AnimeInfoContext<NiblSourceService>
    {
        public string FolderName { get; set; }

        public NiblAnimeInfoContext(ISourceService sourceService)
            : base(sourceService)
        {
        }

        public override async Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable()
        {
            var newEpisodes = await this.SourceService.GetNewEpisodes(this);
            this.Episodes.AddRange(newEpisodes);
            return newEpisodes;
        }
    }

    public class CrunchyrollAnimeInfo : AnimeInfo
    {
    }

    public class NiblAnimeInfo : AnimeInfo
    {
        public ulong PackageNumber { get; set; }
        public string DestinationFolderName { get; set; }
    }

    public abstract class AnimeInfo : IAnimeInfo
    {
        public double EpisodeNumber { get; internal set; }
        public string Name { get; internal set; }
        public ulong MyAnimeListId { get; internal set; }

        // TODO: put that rather into a factory
        public static IAnimeInfo FromDb(Models.Database.Episode animeInfo)
        {
            IAnimeInfo result = null;
            switch (animeInfo.AnimeContext.Type)
            {
                case Models.Database.AnimeContextType.Crunchyroll:
                    result = new CrunchyrollAnimeInfo()
                    {
                        EpisodeNumber = animeInfo.EpisodeNumber,
                        MyAnimeListId = animeInfo.AnimeContext.MyAnimeListId,
                        Name = animeInfo.Name,
                    };
                    break;

                case Models.Database.AnimeContextType.Nibl:
                    result = new NiblAnimeInfo()
                    {
                        DestinationFolderName = animeInfo.AnimeContext.FolderName,
                        EpisodeNumber = animeInfo.EpisodeNumber,
                        MyAnimeListId = animeInfo.AnimeContext.MyAnimeListId,
                        Name = animeInfo.Name,
                        PackageNumber = animeInfo.PackageNumber.Number
                    };
                    break;

                default:
                    break;
            }

            return result;
        }
    }

    public interface IAnimeInfo
    {
        double EpisodeNumber { get; }

        string Name { get; }

        ulong MyAnimeListId { get; }
    }

    public interface ISourceService
    {
        Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context);
    }

    public interface IActionService
    {
        Task Execute(AnimeInfoContext context, IAnimeInfo animeInfo, CancellationToken cancellationToken);
    }
}
