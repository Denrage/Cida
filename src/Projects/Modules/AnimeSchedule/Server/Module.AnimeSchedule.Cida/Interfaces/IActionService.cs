namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface IActionService
    {
        Task Execute(IActionable actionable, int scheduleId, CancellationToken cancellationToken);
    }
}
