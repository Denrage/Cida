using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Interfaces;

public interface IDownloadable
{
    Task<DownloadInformation> GetDownloadInformation();

    bool AlreadyProcessed { get; }
}
