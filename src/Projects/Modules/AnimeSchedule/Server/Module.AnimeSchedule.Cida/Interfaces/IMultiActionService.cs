namespace Module.AnimeSchedule.Cida.Interfaces;

public interface IMultiActionService
{
    Task Execute(IEnumerable<IActionable> actionables, uint scheduleId, CancellationToken cancellationToken);
}
