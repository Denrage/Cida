namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface IActionService
    {
        Task Execute(IActionable actionable, uint scheduleId, CancellationToken cancellationToken);
    }
}
