using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Models;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Schedules;

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
            if (selectedSchedule != value)
            {
                selectedSchedule = value;
                this.RaisePropertyChanged();
                Task.Run(async () =>
                {
                    if (selectedSchedule != null)
                    {
                        selectedSchedule.Animes = (await LoadAnimes(selectedSchedule.ScheduleId)).ToList();
                        SubViewModel = new ScheduleAnimesViewModel(client, selectedSchedule);
                    }
                    else
                    {
                        SubViewModel = null;
                    }
                });
            }
        }
    }

    public ViewModelBase SubViewModel
    {
        get => subViewModel;
        set => this.RaiseAndSetIfChanged(ref subViewModel, value);
    }

    public ScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public async Task LoadAsync()
    {
        var schedules = await client.GetSchedulesAsync(new GetSchedulesRequest());
        Schedules.Clear();
        foreach (var schedule in schedules.Schedules)
        {
            Schedules.Add(new Schedule()
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
        await client.StopScheduleAsync(new StopScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
        });
    }

    public async Task RunSchedule(Schedule schedule)
    {
        await client.ForceRunScheduleAsync(new ForceRunScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
        });
    }

    public async Task StartSchedule(Schedule schedule)
    {
        await client.StartScheduleAsync(new StartScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
        });
    }

    public void Edit()
    {
        var editViewModel = new EditScheduleViewModel(client, selectedSchedule);
        editViewModel.OnClose += OnEditClose;

        SubViewModel = editViewModel;

    }

    public void Create()
    {
        var editViewModel = new EditScheduleViewModel(client, new Schedule());
        editViewModel.OnClose += OnEditClose;

        SubViewModel = editViewModel;

    }

    private async void OnEditClose(Schedule schedule)
    {
        await LoadAsync();
        if (schedule.ScheduleId != default)
        {
            SelectedSchedule = Schedules.FirstOrDefault(x => x.ScheduleId == schedule.ScheduleId);
        }
        else
        {
            SelectedSchedule = null;
        }
    }

    private ScheduleState ConvertState(GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState state)
    {
        return state switch
        {
            GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Running => ScheduleState.Running,
            GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Stopped => ScheduleState.Stopped,
            GetSchedulesResponse.Types.ScheduleItem.Types.ScheduleState.Waiting => ScheduleState.Waiting,
            _ => ScheduleState.Stopped,
        };
    }

    private async Task<IEnumerable<AnimeInfo>> LoadAnimes(int scheduleId)
    {
        var animes = await client.GetAnimesByScheduleAsync(new GetAnimesByScheduleRequest()
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
