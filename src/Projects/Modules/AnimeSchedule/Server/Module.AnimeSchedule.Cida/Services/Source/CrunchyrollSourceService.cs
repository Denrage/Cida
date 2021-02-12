using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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

        private async Task RefreshCache()
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
            finally
            {
                this.cacheSemaphore.Release();
            }
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context)
        {
            await this.RefreshCache();
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
