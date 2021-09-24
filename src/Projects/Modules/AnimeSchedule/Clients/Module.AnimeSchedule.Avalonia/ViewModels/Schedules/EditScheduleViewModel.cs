using Animeschedule;
using Cida.Client.Avalonia.Api;
using Google.Protobuf.WellKnownTypes;
using Module.AnimeSchedule.Avalonia.Models;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Schedules;

public class EditScheduleViewModel : EditViewModel<Schedule>
{
    public DateTimeOffset? SelectedDate
    {
        get => DateTime.SpecifyKind(this.Item.StartDate, DateTimeKind.Local);
        set => this.Item.StartDate = value.HasValue ? value.Value.DateTime : DateTime.MaxValue;
    }

    public TimeSpan? SelectedTime
    {
        get => this.Item.Interval;
        set => this.Item.Interval = value.HasValue ? value.Value : TimeSpan.FromMinutes(30);
    }

    public EditScheduleViewModel(Schedule item)
        : base(item)
    {
    }
}
