using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Source
{
    public class NiblSourceService : ISourceService
    {
        private const long GinpachiId = 21;
        private const string NiblApiUrl = "https://api.nibl.co.uk/nibl/latest?size=100000";
        private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);
        private readonly ConcurrentBag<NiblAnimeResult> cache = new ConcurrentBag<NiblAnimeResult>();
        private readonly SemaphoreSlim cacheSemaphore = new SemaphoreSlim(1);
        private readonly ILogger logger;
        private DateTime lastCacheRefresh = DateTime.MinValue;


        public NiblSourceService(ILogger logger)
        {
            this.logger = logger;
        }

        private async Task RefreshCache()
        {
            await this.cacheSemaphore.WaitAsync();
            try
            {
                if (DateTime.Now - this.lastCacheRefresh > this.cacheDuration)
                {
                    this.logger.Info("Cache not up to date. Refreshing ...");
                    var webClient = new WebClient { Proxy = null };

                    var page = await webClient.DownloadStringTaskAsync(new Uri(NiblApiUrl)).ConfigureAwait(false);
                    var result = JsonSerializer.Deserialize<RequestResult>(page);

                    if (result.Status.ToUpper() == "OK")
                    {
                        this.cache.Clear();

                        foreach (var item in result.Content.Where(x => x.BotId == GinpachiId))
                        {
                            this.cache.Add(item);
                        }


                        this.logger.Info("Cache refreshed");
                    }
                    else
                    {
                        this.logger.Warn($"Error on refreshing cache. Status code is '{result.Status}'");
                    }

                    this.lastCacheRefresh = DateTime.Now;
                }
            }
            finally
            {
                this.cacheSemaphore.Release();
            }
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context)
        {
            if (context is NiblAnimeInfoContext niblAnimeInfoContext)
            {
                return new[] {
                    new NiblAnimeInfo()
                    {
                        DestinationFolderName = "One Piece",
                        Name = "[HorribleSubs] One Piece - 799 [720p].mkv",
                        EpisodeNumber = 799,
                        PackageNumber = 6464,
                    } 
                };

                await this.RefreshCache();
                this.logger.Info($"Checking for new episodes for '{context.Identifier}'");
                var temp = new List<NiblAnimeInfo>();
                foreach (var item in this.cache)
                {
                    if (item.Name.ToUpper().Contains(context.Identifier.ToUpper()))
                    {
                        temp.Add(new NiblAnimeInfo()
                        {
                            EpisodeNumber = item.EpisodeNumber,
                            Name = item.Name,
                            PackageNumber = item.Number,
                            DestinationFolderName = niblAnimeInfoContext.FolderName,
                        });
                    }
                }

                var newEpisodes = temp.Where(x => !context.Episodes.Select(y => y.EpisodeNumber).Contains(x.EpisodeNumber) && x.Name.Contains(context.Filter));
                this.logger.Info($"'{newEpisodes.Count()}' new episodes found for '{context.Identifier}'");

                return newEpisodes;
            }

            return Enumerable.Empty<IAnimeInfo>();
        }
    }

    public partial class RequestResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("content")]
        public NiblAnimeResult[] Content { get; set; }
    }

    public partial class NiblAnimeResult
    {
        [JsonPropertyName("botId")]
        public long BotId { get; set; }

        [JsonPropertyName("number")]
        public long Number { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("sizekbits")]
        public long Sizekbits { get; set; }

        [JsonPropertyName("episodeNumber")]
        public double EpisodeNumber { get; set; }
    }
}
