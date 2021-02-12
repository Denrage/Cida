using System.Collections.Generic;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Services.Source;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class NiblAnimeInfoContext : AnimeInfoContext<NiblSourceService>
    {
        public string FolderName { get; set; }

        public NiblAnimeInfoContext(ISourceService sourceService)
            : base(sourceService)
        {
        }

        public override async Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable()
        {
            var newEpisodes = await this.SourceService.GetNewEpisodes(this);
            this.Episodes.AddRange(newEpisodes);
            return newEpisodes;
        }
    }
}
