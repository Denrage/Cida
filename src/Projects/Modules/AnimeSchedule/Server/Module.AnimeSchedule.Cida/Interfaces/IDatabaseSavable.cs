using Module.AnimeSchedule.Cida.Services;

namespace Module.AnimeSchedule.Cida.Interfaces;

public interface IDatabaseSavable : IActionable
{
    Task SaveToDatabase(AnimeScheduleDbContext context, CancellationToken cancellationToken);
}
