using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Module.Crunchyroll.Cida.Extensions;
using Module.Crunchyroll.Libs.Models.Database;
using Module.Crunchyroll.Libs.Models.Search;

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

            var searchData = JsonSerializer.Deserialize<Libs.Models.Search.Result>(result);
            this.Items = new List<SearchItem>(searchData.Data);
        }


        public async Task<IEnumerable<Libs.Models.Database.Anime>> SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            var result = new List<Libs.Models.Database.Anime>();

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
                var cacheItem = await this.context.Animes.FindAsync(item.Id);
                
                if (cacheItem != null)
                {
                    result.Add(cacheItem);
                }
                else
                {
                    var info = await this.apiService.GetAnimeDetailsAsync(item.Id);
                    
                    if (info != null)
                    {
                        result.Add(info.ToDatabaseModel());
                        await this.context.Animes.AddAsync(info.ToDatabaseModel());
                    }
                    else
                    {
                        this.ignoreIds.Add(item.Id);
                    }
                }
            }

            await this.context.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<Episode>> GetEpisodesAsync(string collectionId)
        {
            var result = new List<Episode>();

            var collection = await this.context.Collections.FindAsync(collectionId);

            if (collection is null)
            {
                collection = (await this.apiService.GetCollectionAsync(collectionId)).ToDatabaseModel();
                await this.context.Collections.AddAsync(collection);
            }
            
            if(collection.Episodes.Count > 0)
            {
                result.AddRange(collection.Episodes);
            }
            else
            {
                var episodes = (await this.apiService.GetEpisodes(collectionId)).Select(x => x.ToDatabaseModel()).ToArray();
                result.AddRange(episodes);
                await this.context.Episodes.AddRangeAsync(episodes);
            }
            
            await this.context.SaveChangesAsync();
            return result;
        }
    }
}