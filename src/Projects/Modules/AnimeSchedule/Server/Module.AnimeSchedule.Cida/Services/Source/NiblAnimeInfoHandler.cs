using System.Text.Json;
using Anilist4Net;
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Source;

public class NiblAnimeInfoHandler : AnimeInfoHandlerBase
{
    private const string NiblApiUrl = "https://api.nibl.co.uk/nibl/search/";

    private readonly Client anilistClient;
    private readonly Dictionary<int, string> Bots = new Dictionary<int, string>()
    {
        { 696, "CR-HOLLAND|NEW" },
        { 21, "Ginpachi-Sensei" },
    };

    public NiblAnimeInfoHandler(ILogger logger, Anilist4Net.Client anilistClient, Func<AnimeScheduleDbContext> getContext)
        : base(logger, getContext)
    {
        this.anilistClient = anilistClient;
    }

    private async Task<IEnumerable<NiblAnimeResult>> Search(string searchTerm, CancellationToken cancellationToken)
    {
        try
        {
            var result = new List<NiblAnimeResult>();
            var httpClient = new HttpClient();
            foreach (var item in this.Bots)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = await httpClient.GetStringAsync(NiblApiUrl + item.Key + "?query=" + searchTerm, cancellationToken).ConfigureAwait(false);
                var botResult = JsonSerializer.Deserialize<RequestResult>(page);

                if (botResult.Status.ToUpper() == "OK")
                {
                    result.AddRange(botResult.Content);
                }
                else
                {
                    this.Logger.Warn($"Error on getting search results. Status code is '{botResult.Status}'");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            this.Logger.Fatal(ex, $"Exception occured searching for '{searchTerm}':");
        }

        return Enumerable.Empty<NiblAnimeResult>();
    }

    public override async Task<IEnumerable<IActionable>> GetNewEpisodes(AnimeInfo info, int scheduleId, CancellationToken cancellationToken)
    {
        var items = await this.Search(info.Identifier, cancellationToken);

        var temp = new List<NiblEpisodeContext>();
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item.Name.ToUpper().Contains(info.Identifier.ToUpper()))
            {
                var package = new Package()
                {
                    Episode = new Episode()
                    {
                        AnimeId = info.Id,
                        EpisodeNumber = item.EpisodeNumber,
                        Name = item.Name,
                    },
                    EpisodeName = item.Name,
                    PackageNumber = (ulong)item.Number,
                    BotName = this.Bots[item.BotId],
                };

                temp.Add(new NiblEpisodeContext(package, this.anilistClient, scheduleId));
            }
        }
        using var dbContext = this.GetContext();
        var filter = await dbContext.AnimeFilters.FindAsync(new object[] { info.Id }, cancellationToken);
        var schedule = await dbContext.Schedules.Include(x => x.Episodes).FirstAsync(x => x.Id == scheduleId, cancellationToken);
        var missingDbEpisodes = await Queryable.Where(dbContext.Episodes, x => x.AnimeId == info.Id).Where(x => !schedule.Episodes.Any(y => y.Name == x.CrunchyrollEpisode.Episode.Name)).ToArrayAsync(cancellationToken);
        var dbEpisodes = await Queryable.Where(dbContext.Episodes, x => x.AnimeId == info.Id).ToArrayAsync(cancellationToken);
        var newEpisodes = temp
            .Where(x => !dbEpisodes.Select(y => y.EpisodeNumber).Contains(x.NiblPackage.Episode.EpisodeNumber))
            .Concat(temp.Where(x => missingDbEpisodes.Any(y => y.Name == x.NiblPackage.Episode.Name)))
            .ToArray();

        foreach (var item in newEpisodes.Where(x => missingDbEpisodes.Select(x => x.Name).Contains(x.NiblPackage.Episode.Name)))
        {
            item.AlreadyProcessed = true;
        }

        if (filter != null)
        {
            newEpisodes = newEpisodes.Where(x => x.NiblPackage.Episode.Name.Contains(filter.Filter)).ToArray();
        }

        newEpisodes = newEpisodes.Distinct(new NiblAnimeInfoEqualityComparer()).ToArray();

        this.Logger.Info($"'{newEpisodes.Length}' new episodes found for '{info.Identifier}'");

        return newEpisodes;
    }

    private class NiblAnimeInfoEqualityComparer : IEqualityComparer<NiblEpisodeContext>
    {
        public bool Equals(NiblEpisodeContext x, NiblEpisodeContext y) => x.NiblPackage.Episode.EpisodeNumber.Equals(y.NiblPackage.Episode.EpisodeNumber);

        public int GetHashCode(NiblEpisodeContext obj) => obj.NiblPackage.Episode.EpisodeNumber.GetHashCode();
    }
}
