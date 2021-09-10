using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Database;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public abstract class AnimeInfo : IAnimeInfo
    {
        public double EpisodeNumber { get; internal set; }

        public string Name { get; internal set; }

        public ulong MyAnimeListId { get; internal set; }

        public abstract Episode ToDb();

        // TODO: put that rather into a factory
        public static IAnimeInfo FromDb(Database.Episode animeInfo)
        {
            IAnimeInfo result = null;
            switch (animeInfo.AnimeContext.Type)
            {
                case Database.AnimeContextType.Crunchyroll:
                    result = new CrunchyrollAnimeInfo()
                    {
                        EpisodeNumber = animeInfo.EpisodeNumber,
                        MyAnimeListId = animeInfo.AnimeContext.MyAnimeListId,
                        Name = animeInfo.Name,
                    };
                    break;

                case Database.AnimeContextType.Nibl:
                    result = new NiblAnimeInfo()
                    {
                        DestinationFolderName = animeInfo.AnimeContext.FolderName,
                        EpisodeNumber = animeInfo.EpisodeNumber,
                        MyAnimeListId = animeInfo.AnimeContext.MyAnimeListId,
                        Name = animeInfo.Name,
                        PackageNumber = animeInfo.PackageNumber.Number
                    };
                    break;

                default:
                    break;
            }

            return result;
        }
    }
}
