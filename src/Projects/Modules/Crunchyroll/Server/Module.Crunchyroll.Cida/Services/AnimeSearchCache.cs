using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Module.Crunchyroll.Cida.Services.Sessions;
using Module.Crunchyroll.Libs.Models.Details;
using Module.Crunchyroll.Libs.Models.Episode;
using Module.Crunchyroll.Libs.Models.Search;
using RestSharp;
using Result = Module.Crunchyroll.Libs.Models.Episode.Result;

namespace Module.Crunchyroll.Cida.Services
{
    public class AnimeSearchCache
    {
        private const string BaseApiUrl = "http://api.crunchyroll.com/";
        private const string SearchEndpoint = "https://crunchyroll.com/ajax/?req=RpcApiSearch_GetSearchCandidates";

        private readonly RestClient apiClient = new RestClient(BaseApiUrl);
        private readonly List<string> ignoreIds = new List<string>();
        private readonly SessionServer[] sessionServers = {
            new CrUnblockerSessionServer(CrUnblockerSessionServer.Version1BaseUrl, CrUnblockerSessionServer.Version1ApiCommand),
            new CrUnblockerSessionServer(CrUnblockerSessionServer.Version2BaseUrl, CrUnblockerSessionServer.Version2ApiCommand),
            new CrunchyrollSessionServer(CrunchyrollSessionServer.Devices[2].DeviceType, CrunchyrollSessionServer.Devices[2].AccessToken),
            new CrunchyrollSessionServer(CrunchyrollSessionServer.Devices[0].DeviceType, CrunchyrollSessionServer.Devices[0].AccessToken),
            new CrunchyrollSessionServer(CrunchyrollSessionServer.Devices[1].DeviceType, CrunchyrollSessionServer.Devices[1].AccessToken),
        };

        private string session;

        public List<SearchItem> Items { get; private set; }

        public List<Libs.Models.Details.Details> SearchCache { get; private set; }


        public AnimeSearchCache()
        {
            this.SearchCache = new List<Libs.Models.Details.Details>();
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
            this.SearchCache.Clear();

            this.SearchCache = JsonSerializer.Deserialize<List<Libs.Models.Details.Details>>(File.ReadAllText("F:\\backup.json"));
        }

        public async Task<IEnumerable<Episode>> GetEpisodes(string id)
        {
            const string listMediaUrlPart = "list_media.0.json";

            var request = new RestRequest(listMediaUrlPart, Method.GET);
            request.AddParameter("session_id", this.session);
            request.AddParameter("series_id", id);
            request.AddParameter("limit", int.MaxValue);

            var response = await this.apiClient.ExecuteTaskAsync(request);

            if (response.IsSuccessful)
            {
                var apiResult = JsonSerializer.Deserialize<Result>(response.Content);
                if (!apiResult.Error)
                {
                    return apiResult.Data;
                }
            }

            return Array.Empty<Episode>();
        }

        public async Task<IEnumerable<Libs.Models.Details.Details>> SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            if (string.IsNullOrEmpty(this.session))
            {
                this.session = await this.GetSessionAsync();
            }

            var result = new List<Libs.Models.Details.Details>();

            (int ratio, SearchItem item)[] ratios = this.Items
                .Where(x => !this.ignoreIds.Contains(x.Id))
                .Select(x => (FuzzySharp.Fuzz.PartialRatio(searchTerm, x.Name.ToLower()), x))
                .ToArray();
            const double percentualThreshold = 0.9;
            var threshold = ratios.Max(x => x.ratio) * percentualThreshold;

            foreach (var item in ratios.Where(x => x.ratio >= threshold).OrderByDescending(x => x.ratio).Select(x => x.item))
            {
                var cacheItem = this.SearchCache.FirstOrDefault(x => x.SeriesId == item.Id);
                if (cacheItem != null)
                {
                    result.Add(cacheItem);
                }
                else
                {
                    var info = await this.GetAnimeDetailsAsync(item.Id);

                    if (info != null)
                    {
                        result.Add(info);
                        this.SearchCache.Add(info);
                    }
                    else
                    {
                        this.ignoreIds.Add(item.Id);
                    }

                }
            }


            return result;
        }

        public async Task<Libs.Models.Details.Details> GetAnimeDetailsAsync(string seriesId)
        {
            const string infoApiCommand = "info.0.json";
            const int maxTries = 1;
            Libs.Models.Details.Details result = null;
            var failed = true;
            var tryCount = 0;

            do
            {
                var request = new RestRequest(infoApiCommand, Method.GET);
                request.AddParameter("series_id", seriesId);
                request.AddParameter("session_id", this.session);
                var response = await this.apiClient.ExecuteTaskAsync(request);
                Libs.Models.Details.Result infoResult = null;
                if (response.IsSuccessful)
                {
                    infoResult = JsonSerializer.Deserialize<Libs.Models.Details.Result>(response.Content);
                    failed = infoResult.Error && infoResult.Code != "forbidden";
                }

                if (failed)
                {
                    tryCount++;
                    if (tryCount <= maxTries)
                    {
                        this.session = await this.GetSessionAsync();
                    }
                }
                else
                {
                    result = infoResult.Details;
                }


            } while (failed && tryCount <= maxTries);

            return result;

        }

        public async Task<string> GetSessionAsync()
        {
            foreach (var server in this.sessionServers)
            {
                var session = await server.GetSession();
                if (session != null && !session.Error)
                {
                    return session.Session.SessionId;
                }
            }

            return null;
        }
    }
}