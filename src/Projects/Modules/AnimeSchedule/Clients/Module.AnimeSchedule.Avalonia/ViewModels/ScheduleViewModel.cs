using Animeschedule;
using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Google.Protobuf.WellKnownTypes;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public class ScheduleViewModel : ModuleViewModel
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private Schedule selectedSchedule;
    private ViewModelBase subViewModel;

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
                        this.selectedSchedule.Animes = (await this.LoadAnimes(this.selectedSchedule.ScheduleId)).ToList();
                        this.SubViewModel = new AnimeScheduleViewModel(this.client, this.selectedSchedule);
                    }
                    else
                    {
                        this.SubViewModel = null;
                    }
                });
            }
        }
    }

    public ViewModelBase SubViewModel
    {
        get => subViewModel;
        set => this.RaiseAndSetIfChanged(ref this.subViewModel, value);
    }

    public ScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public override async Task LoadAsync()
    {
        var schedules = await this.client.GetSchedulesAsync(new GetSchedulesRequest());
        this.Schedules.Clear();
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

    public void Edit()
    {
        var editViewModel = new EditScheduleViewModel(this.client, this.selectedSchedule);
        editViewModel.OnClose += OnEditClose;

        this.SubViewModel = editViewModel;

    }

    public void Create()
    {
        var editViewModel = new EditScheduleViewModel(this.client, new Schedule());
        editViewModel.OnClose += OnEditClose;

        this.SubViewModel = editViewModel;

    }

    private async void OnEditClose(Schedule schedule)
    {
        await this.LoadAsync();
        if (schedule.ScheduleId != default)
        {
            this.SelectedSchedule = this.Schedules.FirstOrDefault(x => x.ScheduleId == schedule.ScheduleId);
        }
        else
        {
            this.SelectedSchedule = null;
        }
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

    public AvaloniaList<AnimeInfo> ScheduleAnimes => new AvaloniaList<AnimeInfo>(this.Schedule?.Animes ?? Enumerable.Empty<AnimeInfo>());

    public AvaloniaList<AnimeInfo> NotInScheduleAnime => new AvaloniaList<AnimeInfo>(this.allAnimes.Where(x => !this.ScheduleAnimes.Select(y => y.Id).Contains(x.Id)));

    public AnimeScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule)
    {
        this.client = client;
        this.schedule = schedule;
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

public class EditScheduleViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private readonly Schedule schedule;

    public event Action<Schedule> OnClose;

    public Schedule Schedule => this.schedule;

    public DateTimeOffset? SelectedDate 
    {  
        get => DateTime.SpecifyKind(this.Schedule.StartDate, DateTimeKind.Local);
        set => this.schedule.StartDate = value.HasValue ? value.Value.DateTime : DateTime.MaxValue;
    }

    public TimeSpan? SelectedTime
    {
        get => this.schedule.Interval;
        set => this.schedule.Interval = value.HasValue ? value.Value : TimeSpan.FromMinutes(30);
    }

    public EditScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule)
    {
        this.client = client;
        this.schedule = schedule;
    }

    public async Task Save()
    {
        var edit = false;
        if (this.schedule.ScheduleId != default)
        {
            edit = true;
        }

        var createResult = await this.client.CreateScheduleAsync(new CreateScheduleRequest()
        {
            Interval = this.schedule.Interval.ToDuration(),
            Name = this.schedule.Name,
            ScheduleId = this.schedule.ScheduleId,
            StartDate = this.schedule.StartDate.ToUniversalTime().ToTimestamp(),
            Override = edit,
        });

        if (createResult.CreateResult != CreateScheduleResponse.Types.Result.Success)
        {
            // TODO: Do something here
        }
        else
        {
            this.schedule.ScheduleId = createResult.ScheduleId;
        }

        this.Close();
    }

    public void Cancel()
    {
        this.Close();
    }

    public void Close()
    {
        Task.Run(() => this.OnClose?.Invoke(this.schedule));
    }
}

public class Schedule
{
    public string Name { get; set; } = string.Empty;

    public int ScheduleId { get; set; }

    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(30);

    public DateTime StartDate { get; set; } = DateTime.Now;

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
