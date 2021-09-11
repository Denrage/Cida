namespace Module.AnimeSchedule.Cida.Services;

public class ScheduleContext
{
    public int ScheduleId { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }
}
