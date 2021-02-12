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
        private const string NiblApiUrl = "https://api.nibl.co.uk/nibl/packs/21/page?size=50&sort=number&direction=DESC&page=";
        private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);
        private readonly ConcurrentBag<NiblAnimeResult> cache = new ConcurrentBag<NiblAnimeResult>();
        private readonly SemaphoreSlim cacheSemaphore = new SemaphoreSlim(1);
        private readonly ILogger logger;
        private DateTime lastCacheRefresh = DateTime.MinValue;

        public NiblSourceService(ILogger logger)
        {
            this.logger = logger;
        }

        private async Task<IEnumerable<NiblAnimeResult>> GetPage(int pageNumber)
        {
            var webClient = new WebClient { Proxy = null };
            var page = await webClient.DownloadStringTaskAsync(new Uri(NiblApiUrl + pageNumber)).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<RequestResult>(page);

            if (result.Status.ToUpper() == "OK")
            {
                return result.Content;
            }
            else
            {
                this.logger.Warn($"Error on getting page. Status code is '{result.Status}'");
            }

            return Enumerable.Empty<NiblAnimeResult>();
        }

        private async Task<bool> RefreshCache()
        {
            await this.cacheSemaphore.WaitAsync();
            try
            {
                if (DateTime.Now - this.lastCacheRefresh > this.cacheDuration)
                {
                    this.logger.Info("Cache not up to date. Refreshing ...");
                    try
                    {
                        var result = await this.GetPage(0);
                        result = result.Concat(await this.GetPage(1));

                        if (result.Any())
                        {
                            this.cache.Clear();

                            foreach (var item in result)
                            {
                                this.cache.Add(item);
                            }

                            this.logger.Info("Cache refreshed");
                        }

                        this.lastCacheRefresh = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        this.logger.Fatal(ex, "Exception occured on refreshing cache");
                        return false;
                    }
                }
            }
            finally
            {
                this.cacheSemaphore.Release();
            }

            return true;
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context)
        {
            if (context is NiblAnimeInfoContext niblAnimeInfoContext)
            {
                if (!(await this.RefreshCache()))
                {
                    return Enumerable.Empty<IAnimeInfo>();
                }

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
