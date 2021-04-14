using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Services.Source;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class CrunchyrollAnimeInfoContext : AnimeInfoContext<CrunchyrollSourceService>
    {
        public CrunchyrollAnimeInfoContext(ISourceService sourceService)
            : base(sourceService)
        {
        }

        public override async Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable(CancellationToken cancellationToken)
        {
            var newEpisodes = await this.SourceService.GetNewEpisodes(this, cancellationToken);
            this.Episodes.AddRange(newEpisodes);
            return newEpisodes;
        }
    }
}
