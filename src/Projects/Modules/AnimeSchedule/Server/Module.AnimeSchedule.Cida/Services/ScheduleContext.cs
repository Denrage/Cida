using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Services;

public class ScheduleContext
{
    public int ScheduleId { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; set; }

    public CancellationTokenSource ForceRunTokenSource { get; set; }

    public Task ScheduleTask {  get; set; }

    public ScheduleState State { get; set; }
}
