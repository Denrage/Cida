using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Source
{
    public class NiblSourceService : ISourceService
    {
        private const string NiblApiUrl = "https://api.nibl.co.uk/nibl/search/";

        private readonly Dictionary<int, string> Bots = new Dictionary<int, string>()
        {
            { 696, "CR-HOLLAND|NEW" },
            { 21, "Ginpachi-Sensei" },
        };

        private readonly ILogger logger;

        public NiblSourceService(ILogger logger)
        {
            this.logger = logger;
        }

        private async Task<IEnumerable<NiblAnimeResult>> Search(string searchTerm, CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<NiblAnimeResult>();
                var webClient = new WebClient { Proxy = null };
                foreach (var item in this.Bots)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var page = await webClient.DownloadStringTaskAsync(new Uri(NiblApiUrl + item.Key + "?query=" + searchTerm)).ConfigureAwait(false);
                    var botResult = JsonSerializer.Deserialize<RequestResult>(page);

                    if (botResult.Status.ToUpper() == "OK")
                    {
                        result.AddRange(botResult.Content);
                    }
                    else
                    {
                        this.logger.Warn($"Error on getting search results. Status code is '{botResult.Status}'");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex, $"Exception occured searching for '{searchTerm}':");
            }

            return Enumerable.Empty<NiblAnimeResult>();
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context, CancellationToken cancellationToken)
        {
            if (context is NiblAnimeInfoContext niblAnimeInfoContext)
            {
                var items = await this.Search(context.Identifier, cancellationToken);

                this.logger.Info($"Checking for new episodes for '{context.Identifier}'");
                var temp = new List<NiblAnimeInfo>();
                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (item.Name.ToUpper().Contains(context.Identifier.ToUpper()))
                    {
                        temp.Add(new NiblAnimeInfo()
                        {
                            EpisodeNumber = item.EpisodeNumber,
                            Name = item.Name,
                            PackageNumber = (ulong)item.Number,
                            DestinationFolderName = niblAnimeInfoContext.FolderName,
                            MyAnimeListId = context.MyAnimeListId,
                            Bot = this.Bots[item.BotId],
                        });
                    }
                }

                var newEpisodes = temp.Where(x => !context.Episodes.Select(y => y.EpisodeNumber).Contains(x.EpisodeNumber) && x.Name.Contains(context.Filter)).Distinct(new NiblAnimeInfoEqualityComparer()).ToArray();
                this.logger.Info($"'{newEpisodes.Length}' new episodes found for '{context.Identifier}'");

                return newEpisodes;
            }

            return Enumerable.Empty<IAnimeInfo>();
        }

        private class NiblAnimeInfoEqualityComparer : IEqualityComparer<NiblAnimeInfo>
        {
            public bool Equals(NiblAnimeInfo x, NiblAnimeInfo y) => x.EpisodeNumber.Equals(y.EpisodeNumber);

            public int GetHashCode(NiblAnimeInfo obj) => obj.EpisodeNumber.GetHashCode();
        }
    }
}
