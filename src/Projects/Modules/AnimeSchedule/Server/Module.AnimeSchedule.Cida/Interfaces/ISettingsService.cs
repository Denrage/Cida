using System;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface ISettingsService
    {
        Task<Settings> Get(CancellationToken cancellationToken);

        Task Set(Settings settings, CancellationToken cancellationToken);

        event Action OnSettingsChanged;
    }
}
