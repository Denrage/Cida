using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;

namespace Module.AnimeSchedule.Cida.Services.Actions
{
    public class DatabaseActionService : IActionService
    {
        private readonly Func<AnimeScheduleDbContext> getContext;

        public DatabaseActionService(Func<AnimeScheduleDbContext> getContext)
        {
            this.getContext = getContext;
        }

        public async Task Execute(AnimeInfoContext animeContext, IAnimeInfo animeInfo, CancellationToken cancellationToken)
        {
            using var context = this.getContext();

            var episode = new Models.Database.Episode()
            {
                AnimeContext = await context.AnimeContexts.FindAsync(animeContext.MyAnimeListId),
                EpisodeNumber = animeInfo.EpisodeNumber,
                Name = animeInfo.Name,
            };

            if (animeInfo is CrunchyrollAnimeInfo crunchyrollAnimeInfo)
            {
                episode.Name = crunchyrollAnimeInfo.SeriesTitle + " - " + "Episode " +
                               crunchyrollAnimeInfo.EpisodeNumber + " - " + crunchyrollAnimeInfo.Name;
            }

            if (animeInfo is NiblAnimeInfo niblAnimeInfo)
            {
                var packageNumber = new Models.Database.PackageNumber()
                {
                    Episode = episode,
                    Name = animeInfo.Name,
                    Number = niblAnimeInfo.PackageNumber,
                };

                episode.PackageNumber = packageNumber;

                await context.PackageNumbers.AddAsync(packageNumber, cancellationToken);
            }

            await context.Episodes.AddAsync(episode, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
