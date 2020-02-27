using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
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
        private readonly CrunchyrollApiService apiService;
        private const string SearchEndpoint = "https://crunchyroll.com/ajax/?req=RpcApiSearch_GetSearchCandidates";

        private readonly List<string> ignoreIds = new List<string>();
        private CrunchyrollDbContext context;

        public List<SearchItem> Items { get; set; }

        public AnimeSearchCache(string connectionString, CrunchyrollApiService apiService)
        {
            this.apiService = apiService;
            this.context = new CrunchyrollDbContext(connectionString);
            this.context.Database.EnsureCreated();
            this.Refresh();
        }

        public void Refresh()
        {
            using var webClient = new WebClient();
            var result = webClient.DownloadString(SearchEndpoint);
            var prefixLength = "/*-secure-".Length;
            var suffixLength = "*/".Length;
            result = result.Substring(prefixLength, result.Length - prefixLength - suffixLength);

            var searchData = JsonSerializer.Deserialize<Result>(result);
            this.Items = new List<SearchItem>(searchData.Data);
        }


        public async Task<IEnumerable<Anime>> SearchAsync(string searchTerm)
        {
            try
            {
                Console.WriteLine("Start search with " + searchTerm);
                searchTerm = searchTerm.ToLower();
                var result = new List<Anime>();

                (int ratio, SearchItem item)[] ratios = this.Items
                    .Where(x => !this.ignoreIds.Contains(x.Id))
                    .Select(x => (FuzzySharp.Fuzz.PartialRatio(searchTerm, x.Name.ToLower()), x))
                    .ToArray();

                const double percentualThreshold = 0.9;
                var threshold = ratios.Max(x => x.ratio) * percentualThreshold;

                foreach (var item in ratios
                    .Where(x => x.ratio >= threshold)
                    .OrderByDescending(x => x.ratio)
                    .Select(x => x.item))
                {
                    var cacheItem = await this.context.Animes
                        .Include(x => x.Landscape)
                        .Include(x => x.Portrait)
                        .FirstOrDefaultAsync(x => x.Id == item.Id);

                    if (cacheItem != null)
                    {
                        result.Add(cacheItem);
                    }
                    else
                    {
                        Console.WriteLine("Getting info of " + item.Name);
                        var info = await this.apiService.GetAnimeDetailsAsync(item.Id);
                        Console.WriteLine("Got info of " + item.Name);

                        if (info != null)
                        {
                            result.Add(info.ToDatabaseModel());
                            await this.context.Animes.AddAsync(info.ToDatabaseModel());
                            await this.context.SaveChangesAsync();
                        }
                        else
                        {
                            this.ignoreIds.Add(item.Id);
                        }
                    }
                }
                Console.WriteLine("End search with " + searchTerm);
                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Episode>> GetEpisodesAsync(string collectionId)
        {
            var result = new List<Episode>();

            var collection = await this.context.Collections
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.Id == collectionId);

            if (collection is null)
            {
                collection = (await this.apiService.GetCollectionAsync(collectionId)).ToDatabaseModel();
                await this.context.Collections.AddAsync(collection);
                await this.context.SaveChangesAsync();
            }

            if (collection.Episodes != null && collection.Episodes.Count > 0)
            {
                result.AddRange(collection.Episodes);
            }
            else
            {
                var episodes = (await this.apiService.GetEpisodes(collectionId)).Select(x => x.ToDatabaseModel()).ToArray();
                result.AddRange(episodes);
                await this.context.Episodes.AddRangeAsync(episodes);
                await this.context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<IEnumerable<Collection>> GetCollectionsAsync(string seriesId)
        {
            var result = new List<Collection>();

            var series = await this.context.Animes
                .Include(x => x.Collections)
                .FirstOrDefaultAsync(x => x.Id == seriesId);

            if (series is null)
            {
                series = (await this.apiService.GetAnimeDetailsAsync(seriesId)).ToDatabaseModel();
                await this.context.Animes.AddAsync(series);
                await this.context.SaveChangesAsync();
            }

            if (series.Collections != null && series.Collections.Count > 0)
            {
                result.AddRange(series.Collections);
            }
            else
            {
                var collections = (await this.apiService.GetAnimeCollectionsAsync(seriesId)).Select(x => x.ToDatabaseModel()).ToArray();
                result.AddRange(collections);
                await this.context.Collections.AddRangeAsync(collections);
                await this.context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<IEnumerable<StreamInformation>> GetStream(string mediaId, string language)
        {
            var result = (await this.apiService.GetStreamUrl(mediaId, language));
            return result.Streams;
        }
    }
}