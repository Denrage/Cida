using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cida.Api;
using Microsoft.EntityFrameworkCore;
using Module.Crunchyroll.Cida.Extensions;
using Module.Crunchyroll.Libs.Models.Database;
using Module.Crunchyroll.Libs.Models.Episode;
using Module.Crunchyroll.Libs.Models.Search;
using Episode = Module.Crunchyroll.Libs.Models.Database.Episode;

namespace Module.Crunchyroll.Cida.Services
{
    public class AnimeSearchCache
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;
        private readonly CrunchyrollApiService apiService;
        private const string SearchEndpoint = "https://crunchyroll.com/ajax/?req=RpcApiSearch_GetSearchCandidates";

        private readonly List<string> ignoreIds = new List<string>();

        public List<SearchItem> Items { get; set; }

        public AnimeSearchCache(string connectionString, IDatabaseProvider databaseProvider, CrunchyrollApiService apiService)
        {
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
            this.apiService = apiService;
            using (var context = this.GetContext())
            {
                context.Database.EnsureCreated();
            }
            Task.Run(async () => await this.Refresh());
        }

        public CrunchyrollDbContext GetContext() => new CrunchyrollDbContext(this.connectionString, this.databaseProvider);

        public async Task Refresh()
        {
            var clearanceHandler = new CloudFlareUtilities.ClearanceHandler();
            using var httpClient = new HttpClient(clearanceHandler);

            var httpResult = await httpClient.GetAsync(SearchEndpoint);
            var result = await httpResult.Content.ReadAsStringAsync();
            var prefixLength = "/*-secure-".Length;
            var suffixLength = "*/".Length;
            result = result.Substring(prefixLength, result.Length - prefixLength - suffixLength);

            var searchData = JsonSerializer.Deserialize<Result>(result);
            this.Items = new List<SearchItem>(searchData.Data);
        }


        public async Task<IEnumerable<Anime>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            searchTerm = searchTerm.ToLower();
            var result = new List<Anime>();

            (int ratio, SearchItem item)[] ratios = this.Items
                .Where(x => !this.ignoreIds.Contains(x.Id))
                .Select(x => (FuzzySharp.Fuzz.PartialRatio(searchTerm, x.Name.ToLower()), x))
                .ToArray();

            const double percentualThreshold = 0.9;
            var threshold = ratios.Max(x => x.ratio) * percentualThreshold;

            const int maxSearchResults = 50;

            using var context = this.GetContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            foreach (var item in ratios
                .Where(x => x.ratio >= threshold)
                .OrderByDescending(x => x.ratio)
                .Select(x => x.item)
                .Take(maxSearchResults))
            {
                cancellationToken.ThrowIfCancellationRequested();


                var cacheItem = await context.Animes
                    .Include(x => x.Landscape)
                    .Include(x => x.Portrait)
                    .FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken);

                if (cacheItem != null)
                {
                    result.Add(cacheItem);
                }
                else
                {
                    var info = await this.apiService.GetAnimeDetailsAsync(item.Id, cancellationToken);

                    if (info != null)
                    {
                        result.Add(info.ToDatabaseModel());
                        await context.Animes.AddAsync(info.ToDatabaseModel(), cancellationToken);
                    }
                    else
                    {
                        this.ignoreIds.Add(item.Id);
                    }
                }
            }

            context.ChangeTracker.DetectChanges();
            await context.SaveChangesAsync(cancellationToken);


            return result;
        }

        public async Task<IEnumerable<Episode>> GetEpisodesAsync(string collectionId, CancellationToken cancellationToken = default)
        {
            var result = new List<Episode>();
            using var context = this.GetContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            var collection = await context.Collections
                .Include(x => x.Episodes)
                .ThenInclude(x => x.Image)
                .FirstOrDefaultAsync(x => x.Id == collectionId, cancellationToken);

            if (collection is null)
            {
                collection = (await this.apiService.GetCollectionAsync(collectionId, cancellationToken)).ToDatabaseModel();
                await context.Collections.AddAsync(collection, cancellationToken);
            }

            if (collection.Episodes != null && collection.Episodes.Count > 0)
            {
                result.AddRange(collection.Episodes);
            }
            else
            {
                var episodes = (await this.apiService.GetEpisodes(collectionId, cancellationToken)).Select(x => x.ToDatabaseModel())
                    .ToArray();
                result.AddRange(episodes);
                await context.Episodes.AddRangeAsync(episodes);
            }

            context.ChangeTracker.DetectChanges();
            await context.SaveChangesAsync(cancellationToken);

            return result;
        }

        public async Task<IEnumerable<Collection>> GetCollectionsAsync(string seriesId, CancellationToken cancellationToken = default)
        {
            var result = new List<Collection>();
            using var context = this.GetContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            var series = await context.Animes
                .Include(x => x.Collections)
                .FirstOrDefaultAsync(x => x.Id == seriesId, cancellationToken);

            if (series is null)
            {
                series = (await this.apiService.GetAnimeDetailsAsync(seriesId, cancellationToken)).ToDatabaseModel();
                await context.Animes.AddAsync(series, cancellationToken);
            }

            if (series.Collections != null && series.Collections.Count > 0)
            {
                result.AddRange(series.Collections);
            }
            else
            {
                var collections = (await this.apiService.GetAnimeCollectionsAsync(seriesId, cancellationToken))
                    .Select(x => x.ToDatabaseModel()).ToArray();
                result.AddRange(collections);
                await context.Collections.AddRangeAsync(collections);
            }

            context.ChangeTracker.DetectChanges();    
            await context.SaveChangesAsync(cancellationToken);
            
            return result;
        }

        public async Task<IEnumerable<StreamInformation>> GetStream(string mediaId, string language, CancellationToken cancellationToken = default)
        {
            var result = (await this.apiService.GetStreamUrl(mediaId, language, cancellationToken));
            return result.Streams;
        }
    }
}