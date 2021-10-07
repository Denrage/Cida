using Module.AnimeSchedule.Cida.Models;
using Module.AnimeSchedule.Cida.Services;

namespace Module.AnimeSchedule.Cida.Interfaces;

public interface IDownloadable
{
    Task<DownloadInformation> GetDownloadInformation(Func<AnimeScheduleDbContext> getContext, CancellationToken cancellationToken);

    bool AlreadyProcessed { get; }
}
