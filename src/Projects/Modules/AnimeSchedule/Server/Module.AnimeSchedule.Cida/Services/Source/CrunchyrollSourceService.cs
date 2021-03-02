using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Source
{
    public class CrunchyrollSourceService : ISourceService
    {
        private const string CrunchyrollRssUrl = "http://feeds.feedburner.com/crunchyroll/rss/anime";
        private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);
        private readonly ConcurrentBag<SyndicationItem> cache = new ConcurrentBag<SyndicationItem>();
        private readonly SemaphoreSlim cacheSemaphore = new SemaphoreSlim(1);
        private readonly ILogger logger;
        private DateTime lastCacheRefresh = DateTime.MinValue;

        public CrunchyrollSourceService(ILogger logger)
        {
            this.logger = logger;
        }

        private async Task<bool> RefreshCache()
        {
            await this.cacheSemaphore.WaitAsync();
            try
            {
                if (DateTime.Now - this.lastCacheRefresh > this.cacheDuration)
                {
                    this.logger.Info("Cache not up to date. Refreshing ...");
                    using var reader = XmlReader.Create(CrunchyrollRssUrl);
                    var feed = SyndicationFeed.Load(reader);

                    this.cache.Clear();

                    foreach (var item in feed.Items)
                    {
                        this.cache.Add(item);
                    }

                    this.lastCacheRefresh = DateTime.Now;

                    this.logger.Info("Cache refreshed");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to refresh cache!");
                return false;
            }
            finally
            {
                this.cacheSemaphore.Release();
            }
            return true;
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context)
        {
            if (!await this.RefreshCache())
            {
                this.logger.Warn("Couldn't refresh cache, retrying in the next cycle");
            }
            this.logger.Info($"Checking for new episodes for '{context.Identifier}'");
            var temp = new List<CrunchyrollAnimeInfo>();
            foreach (var item in this.cache)
            {
                if (item.Title.Text.ToUpper().Contains(context.Identifier.ToUpper()))
                {
                    temp.Add(new CrunchyrollAnimeInfo()
                    {
                        EpisodeNumber = item.ElementExtensions.FirstOrDefault(x => x.OuterName == "episodeNumber").GetObject<double>(),
                        Name = item.Title.Text,
                        MyAnimeListId = context.MyAnimeListId,
                    });
                }
            }

            var newEpisodes = temp.Where(x => !context.Episodes.Select(y => y.EpisodeNumber).Contains(x.EpisodeNumber)).ToArray();
            this.logger.Info($"'{newEpisodes.Length}' new episodes found for '{context.Identifier}'");

            return newEpisodes;
        }
    }
}
