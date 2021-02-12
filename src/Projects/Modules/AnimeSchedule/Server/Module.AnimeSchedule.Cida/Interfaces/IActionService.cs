using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Models.Schedule;

namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface IActionService
    {
        Task Execute(AnimeInfoContext context, IAnimeInfo animeInfo, CancellationToken cancellationToken);
    }
}
