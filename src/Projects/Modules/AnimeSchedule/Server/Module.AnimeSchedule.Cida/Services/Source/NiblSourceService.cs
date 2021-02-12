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
        private const string NiblApiUrl = "https://api.nibl.co.uk/nibl/search/21?query=";
        private readonly ILogger logger;

        public NiblSourceService(ILogger logger)
        {
            this.logger = logger;
        }

        private async Task<IEnumerable<NiblAnimeResult>> Search(string searchTerm)
        {
            try
            {
                var webClient = new WebClient { Proxy = null };
                var page = await webClient.DownloadStringTaskAsync(new Uri(NiblApiUrl + searchTerm)).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<RequestResult>(page);

                if (result.Status.ToUpper() == "OK")
                {
                    return result.Content;
                }
                else
                {
                    this.logger.Warn($"Error on getting search results. Status code is '{result.Status}'");
                }
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex, $"Exception occured searching for '{searchTerm}':");
            }

            return Enumerable.Empty<NiblAnimeResult>();
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context)
        {
            if (context is NiblAnimeInfoContext niblAnimeInfoContext)
            {
                var items = await this.Search(context.Identifier);

                this.logger.Info($"Checking for new episodes for '{context.Identifier}'");
                var temp = new List<NiblAnimeInfo>();
                foreach (var item in items)
                {
                    if (item.Name.ToUpper().Contains(context.Identifier.ToUpper()))
                    {
                        temp.Add(new NiblAnimeInfo()
                        {
                            EpisodeNumber = item.EpisodeNumber,
                            Name = item.Name,
                            PackageNumber = (ulong)item.Number,
                            DestinationFolderName = niblAnimeInfoContext.FolderName,
                            MyAnimeListId = context.MyAnimeListId,
                        });
                    }
                }

                var newEpisodes = temp.Where(x => !context.Episodes.Select(y => y.EpisodeNumber).Contains(x.EpisodeNumber) && x.Name.Contains(context.Filter)).ToArray();
                this.logger.Info($"'{newEpisodes.Length}' new episodes found for '{context.Identifier}'");

                return newEpisodes;
            }

            return Enumerable.Empty<IAnimeInfo>();
        }
    }
}
