using Animeschedule;
using Cida.Client.Avalonia.Api;
using Google.Protobuf.WellKnownTypes;
using Module.AnimeSchedule.Avalonia.Models;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Schedules;

public class EditScheduleViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private readonly Schedule schedule;

    public event Action<Schedule> OnClose;

    public Schedule Schedule => schedule;

    public DateTimeOffset? SelectedDate
    {
        get => DateTime.SpecifyKind(Schedule.StartDate, DateTimeKind.Local);
        set => schedule.StartDate = value.HasValue ? value.Value.DateTime : DateTime.MaxValue;
    }

    public TimeSpan? SelectedTime
    {
        get => schedule.Interval;
        set => schedule.Interval = value.HasValue ? value.Value : TimeSpan.FromMinutes(30);
    }

    public EditScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule)
    {
        this.client = client;
        this.schedule = schedule;
    }

    public async Task Save()
    {
        var edit = false;
        if (schedule.ScheduleId != default)
        {
            edit = true;
        }

        var createResult = await client.CreateScheduleAsync(new CreateScheduleRequest()
        {
            Interval = schedule.Interval.ToDuration(),
            Name = schedule.Name,
            ScheduleId = schedule.ScheduleId,
            StartDate = schedule.StartDate.ToUniversalTime().ToTimestamp(),
            Override = edit,
        });

        if (createResult.CreateResult != CreateScheduleResponse.Types.Result.Success)
        {
            // TODO: Do something here
        }
        else
        {
            schedule.ScheduleId = createResult.ScheduleId;
        }

        Close();
    }

    public void Cancel()
    {
        Close();
    }

    public void Close()
    {
        Task.Run(() => OnClose?.Invoke(schedule));
    }
}
