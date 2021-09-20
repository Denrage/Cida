using Animeschedule;
using Cida.Client.Avalonia.Api;
using Google.Protobuf.WellKnownTypes;
using Module.AnimeSchedule.Avalonia.Models;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

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
