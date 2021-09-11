namespace Module.AnimeSchedule.Cida.Services;

public class ScheduleContext
{
    public uint ScheduleId { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }
}
