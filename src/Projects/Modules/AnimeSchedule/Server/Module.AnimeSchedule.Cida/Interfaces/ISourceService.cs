using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Models.Schedule;

namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface ISourceService
    {
        Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context, CancellationToken cancellationToken);
    }
}
