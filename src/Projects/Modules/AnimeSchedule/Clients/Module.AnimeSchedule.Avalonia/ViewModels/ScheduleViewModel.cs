using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Models;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public class ScheduleViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private Schedule selectedSchedule;
    private ViewModelBase subViewModel;

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
                        this.SubViewModel = new ScheduleAnimesViewModel(this.client, this.selectedSchedule);
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

    public async Task LoadAsync()
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
