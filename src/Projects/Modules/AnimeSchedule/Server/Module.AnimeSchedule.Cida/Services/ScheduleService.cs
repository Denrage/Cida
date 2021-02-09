using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Services.Source;

namespace Module.AnimeSchedule.Cida.Services
{
    public class ScheduleService
    {
        private List<IActionService> actionServices;
        private List<(Schedule schedule, Task task)> schedules = new List<(Schedule, Task)>();

        public ScheduleService(IEnumerable<IActionService> actionServices)
        {
            this.actionServices = new List<IActionService>(actionServices);
        }

        public void StartSchedule(Schedule schedule)
        {
            var scheduleTask = new Task(async () => await this.Schedule(schedule), schedule.CancellationToken, TaskCreationOptions.LongRunning);
            scheduleTask.Start();
            this.schedules.Add((schedule, scheduleTask));
        }

        private async Task Schedule(Schedule schedule)
        {
            while (true)
            {
                schedule.CancellationToken.ThrowIfCancellationRequested();

                if (DateTime.Now > schedule.StartDate)
                {
                    foreach (var item in schedule.Animes)
                    {
                        var newEpisodes = await item.NewEpisodesAvailable();

                        foreach (var episode in newEpisodes)
                        {
                            foreach (var actionService in this.actionServices)
                            {
                                await actionService.Execute(episode, schedule.CancellationToken);
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
        public DateTime StartDate { get; set; }

        public List<AnimeInfoContext> Animes { get; set; }

        public TimeSpan Interval { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }

    public abstract class AnimeInfoContext
    {
        protected ISourceService SourceService { get; }

        public string Identifier { get; set; }

        public long MyAnimeListId { get; set; }

        public string Filter { get; set; }

        public List<IAnimeInfo> Episodes { get; set; } = new List<IAnimeInfo>();

        protected AnimeInfoContext(ISourceService sourceService)
        {
            this.SourceService = sourceService;
        }

        public abstract Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable();
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

    public class CrunchyrollAnimeInfo : IAnimeInfo
    {
        public double EpisodeNumber { get; internal set; }
        public string Name { get; internal set; }
    }

    public class NiblAnimeInfo : IAnimeInfo
    {
        public double EpisodeNumber { get; internal set; }
        public string Name { get; internal set; }
        public long PackageNumber { get; set; }
        public string DestinationFolderName { get; set; }
    }

    public interface IAnimeInfo
    {
        double EpisodeNumber { get; }

        string Name { get; }
    }

    public interface ISourceService
    {
        Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context);
    }

    public interface IActionService
    {
        Task Execute(IAnimeInfo animeInfo, CancellationToken cancellationToken);
    }


}
