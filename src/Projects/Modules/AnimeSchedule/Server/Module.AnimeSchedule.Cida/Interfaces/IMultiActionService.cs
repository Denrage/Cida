namespace Module.AnimeSchedule.Cida.Interfaces;

public interface IMultiActionService
{
    Task Execute(IEnumerable<IActionable> actionables, int scheduleId, CancellationToken cancellationToken);
}
