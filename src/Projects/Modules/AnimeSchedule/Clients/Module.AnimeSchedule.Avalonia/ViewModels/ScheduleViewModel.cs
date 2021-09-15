using Animeschedule;
using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public class ScheduleViewModel : ModuleViewModel
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private Schedule selectedSchedule;
    private AnimeScheduleViewModel selectedAnimeSchedule;

    public override string Name => "Anime Schedule";

    public AvaloniaList<Schedule> Schedules { get; } = new AvaloniaList<Schedule>();

    public Schedule SelectedSchedule
    {
        get => selectedSchedule;
        set
        {
            if (this.selectedSchedule != value)
            {
                this.selectedSchedule = value;
                this.RaisePropertyChanged();
                Task.Run(async () =>
                {
                    if (this.selectedSchedule != null)
                    {
                        this.selectedSchedule.Animes = (await this.LoadAnimes(value.ScheduleId)).ToList();
                        this.SelectedAnimeSchedule = new AnimeScheduleViewModel(this.client);
                        this.SelectedAnimeSchedule.Schedule = this.selectedSchedule;
                    }
                });
            }
        }
    }

    public AnimeScheduleViewModel SelectedAnimeSchedule
    {
        get => selectedAnimeSchedule;
        set => this.RaiseAndSetIfChanged(ref this.selectedAnimeSchedule, value);
    }

    public ScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public override async Task LoadAsync()
    {
        var schedules = await this.client.GetSchedulesAsync(new GetSchedulesRequest());
        foreach (var schedule in schedules.Schedules)
        {
            this.Schedules.Add(new Schedule()
            {
                Name = schedule.Name,
                ScheduleId = schedule.ScheduleId,
                Interval = schedule.Interval.ToTimeSpan(),
                StartDate = schedule.StartDate.ToDateTime().ToLocalTime(),
                State = ConvertState(schedule.State),
            });
        }
    }

    public async Task StopSchedule(Schedule schedule)
    {
        await this.client.StopScheduleAsync(new StopScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
        });
    }

    public async Task RunSchedule(Schedule schedule)
    {
        await this.client.ForceRunScheduleAsync(new ForceRunScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
        });
    }

    public async Task StartSchedule(Schedule schedule)
    {
        await this.client.StartScheduleAsync(new StartScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
        });
    }

    private ScheduleState ConvertState(Animeschedule.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState state)
    {
        return state switch
        {
            Animeschedule.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Running => ScheduleState.Running,
            Animeschedule.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Stopped => ScheduleState.Stopped,
            Animeschedule.GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Waiting => ScheduleState.Waiting,
            _ => ScheduleState.Stopped,
        };
    }

    private async Task<IEnumerable<AnimeInfo>> LoadAnimes(int scheduleId)
    {
        var animes = await this.client.GetAnimesByScheduleAsync(new GetAnimesByScheduleRequest()
        {
            ScheduleId = scheduleId,
        });

        return animes.Animes.Select(x => new AnimeInfo()
        {
            Identifier = x.Identifier,
            Id = x.Id,
        });
    }
}

public class AnimeScheduleViewModel : ViewModelBase
{
    private readonly List<AnimeInfo> allAnimes = new List<AnimeInfo>();
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private Schedule schedule;

    public Schedule Schedule
    {
        get => schedule;
        set
        {
            if (this.schedule != value)
            {
                this.schedule = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ScheduleAnimes));
                this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
            }
        }
    }

    public IEnumerable<AnimeInfo> ScheduleAnimes => this.Schedule?.Animes ?? Enumerable.Empty<AnimeInfo>();

    public IEnumerable<AnimeInfo> NotInScheduleAnime => this.allAnimes.Where(x => !this.ScheduleAnimes.Select(y => y.Id).Contains(x.Id));

    public AnimeScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        Task.Run(async () =>
        {
            var animes = await this.client.GetAnimesAsync(new GetAnimesRequest());

            this.allAnimes.AddRange(animes.Animes.Select(x => new AnimeInfo()
            {
                Identifier = x.Identifier,
                Id = x.Id,
            }));
            this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
        });
        this.client = client;
    }

    public async Task AssignAnime(AnimeInfo animeInfo)
    {
        var assignResult = await this.client.AssignAnimeInfoToScheduleAsync(new AssignAnimeInfoToScheduleRequest()
        {
            AnimeId = animeInfo.Id,
            ScheduleId = this.Schedule.ScheduleId,
        });

        if (assignResult.AssignResult == AssignAnimeInfoToScheduleResponse.Types.Result.Success)
        {
            this.Schedule.Animes.Add(animeInfo);
            this.RaisePropertyChanged(nameof(this.ScheduleAnimes));
            this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
        }
    }

    public async Task UnassignAnime(AnimeInfo animeInfo)
    {
        var unassignResult = await this.client.UnassignAnimeInfoToScheduleAsync(new UnassignAnimeInfoToScheduleRequest()
        {
            AnimeId = animeInfo.Id,
            ScheduleId = this.Schedule.ScheduleId,
        });

        if (unassignResult.AssignResult == UnassignAnimeInfoToScheduleResponse.Types.Result.Success)
        {
            this.Schedule.Animes.Remove(animeInfo);
            this.RaisePropertyChanged(nameof(this.ScheduleAnimes));
            this.RaisePropertyChanged(nameof(this.NotInScheduleAnime));
        }
    }
}

public class Schedule
{
    public string Name { get; set; }

    public int ScheduleId { get; set; }

    public TimeSpan Interval { get; set; }

    public DateTime StartDate { get; set; }

    public ScheduleState State { get; set; }

    public List<AnimeInfo> Animes { get; set; }
}

public enum ScheduleState
{
    Running,
    Waiting,
    Stopped,
}

public class AnimeInfo
{
    public int Id { get; set; }

    public string Identifier { get; set; }
}
