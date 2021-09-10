using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Database;
using Module.AnimeSchedule.Cida.Services.Source;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public abstract class AnimeInfoContext
    {
        protected ISourceService SourceService { get; }

        public string Identifier { get; set; }

        public ulong MyAnimeListId { get; set; }

        public string Filter { get; set; }

        public List<IAnimeInfo> Episodes { get; set; } = new List<IAnimeInfo>();

        protected AnimeInfoContext(ISourceService sourceService)
        {
            this.SourceService = sourceService;
        }

        public abstract Task<IEnumerable<IAnimeInfo>> NewEpisodesAvailable(CancellationToken cancellationToken);

        // TODO: put that rather into a factory
        public static AnimeInfoContext FromDb(Models.Database.AnimeContext context, CrunchyrollSourceService crunchyrollSourceService, NiblSourceService niblSourceService)
        {
            AnimeInfoContext result = null;
            switch (context.Type)
            {
                case Models.Database.AnimeContextType.Crunchyroll:
                    result = new CrunchyrollAnimeInfoContext(crunchyrollSourceService)
                    {
                        Episodes = context.Episodes.Select(x => AnimeInfo.FromDb(x)).ToList(),
                        Filter = context.Filter,
                        Identifier = context.Identifier,
                        MyAnimeListId = context.MyAnimeListId,
                    };
                    break;

                case Models.Database.AnimeContextType.Nibl:
                    result = new NiblAnimeInfoContext(niblSourceService)
                    {
                        Episodes = context.Episodes.Select(x => AnimeInfo.FromDb(x)).ToList(),
                        Filter = context.Filter,
                        Identifier = context.Identifier,
                        MyAnimeListId = context.MyAnimeListId,
                        FolderName = context.FolderName,
                    };
                    break;

                default:
                    break;
            }

            return result;
        }

        public Database.AnimeContext ToDb()
        {
            return new Database.AnimeContext()
            {
                Episodes = this.Episodes.Select(x => x.ToDb()).ToList(),
                Filter = this.Filter,
                FolderName = this is NiblAnimeInfoContext niblContext ? niblContext.FolderName : string.Empty,
                Identifier = this.Identifier,
                MyAnimeListId = this.MyAnimeListId,
                Type = this is NiblAnimeInfoContext ? AnimeContextType.Nibl : AnimeContextType.Crunchyroll,
            };
        }
    }
}
