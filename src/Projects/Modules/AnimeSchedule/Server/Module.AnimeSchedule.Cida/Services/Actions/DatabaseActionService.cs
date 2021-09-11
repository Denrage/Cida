using Module.AnimeSchedule.Cida.Interfaces;

namespace Module.AnimeSchedule.Cida.Services.Actions;

public class DatabaseActionService : IActionService
{
    private readonly Func<AnimeScheduleDbContext> getContext;

    public DatabaseActionService(Func<AnimeScheduleDbContext> getContext)
    {
        this.getContext = getContext;
    }

    public async Task Execute(IActionable actionable, uint scheduleId, CancellationToken cancellationToken)
    {
        if (actionable is IDatabaseSavable databaseSavable)
        {
            using var context = this.getContext();

            await databaseSavable.SaveToDatabase(context, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
