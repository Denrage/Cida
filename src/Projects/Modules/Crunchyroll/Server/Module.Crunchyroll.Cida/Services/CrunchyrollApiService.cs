using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Module.Crunchyroll.Cida.Services.Sessions;
using Module.Crunchyroll.Libs.Models.Collection;
using Module.Crunchyroll.Libs.Models.Episode;
using RestSharp;

namespace Module.Crunchyroll.Cida.Services
{
    public class CrunchyrollApiService
    {
        private const string BaseApiUrl = "http://api.crunchyroll.com/";
        private readonly RestClient apiClient = new RestClient(BaseApiUrl);
        private string session;

        private readonly SessionServer[] sessionServers =
        {
            new CrUnblockerSessionServer(CrUnblockerSessionServer.Version1BaseUrl,
                CrUnblockerSessionServer.Version1ApiCommand),
            new CrUnblockerSessionServer(CrUnblockerSessionServer.Version2BaseUrl,
                CrUnblockerSessionServer.Version2ApiCommand),
            new CrunchyrollSessionServer(CrunchyrollSessionServer.Devices[2].DeviceType,
                CrunchyrollSessionServer.Devices[2].AccessToken),
            new CrunchyrollSessionServer(CrunchyrollSessionServer.Devices[0].DeviceType,
                CrunchyrollSessionServer.Devices[0].AccessToken),
            new CrunchyrollSessionServer(CrunchyrollSessionServer.Devices[1].DeviceType,
                CrunchyrollSessionServer.Devices[1].AccessToken),
        };
        
        public CrunchyrollApiService()
        {
            Task.Run(async () => await this.InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(this.session))
            {
                this.session = await this.GetSessionAsync();
            }
        }

        public async Task<Collection> GetCollectionAsync(string collectionId)
        {
            const string detailCollectionsUrlPart = "info.0.json";

            var request = new RestRequest(detailCollectionsUrlPart, Method.GET);
            request.AddParameter("collection_id", collectionId);
            request.AddParameter("limit", int.MaxValue);

            return await this.ExecuteRequestAsync<Collection>(request);
        }

        public async Task<IEnumerable<Libs.Models.Collection.Collection>> GetAnimeCollectionsAsync(int seriesId)
        {
            const string listCollectionsUrlPart = "list_collections.0.json";

            var request = new RestRequest(listCollectionsUrlPart, Method.GET);
            request.AddParameter("series_id", seriesId);
            request.AddParameter("limit", int.MaxValue);

            return ((IEnumerable<Libs.Models.Collection.Collection>) await this
                       .ExecuteRequestAsync<List<Libs.Models.Collection.Collection>>(request)) ??
                   await Task.FromResult(Array.Empty<Libs.Models.Collection.Collection>());
        }

        private async Task<T> ExecuteRequestAsync<T>(RestRequest request)
            where T : class
        {
            const int maxTries = 1;

            var failed = true;
            var tryCount = 0;

            do
            {
                request.AddOrUpdateParameter("session_id", this.session);
                var response = await this.apiClient.ExecuteTaskAsync(request);
                Libs.Models.Result<T> infoResult = null;

                if (response.IsSuccessful)
                {
                    infoResult = JsonSerializer.Deserialize<Libs.Models.Result<T>>(response.Content);
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
                    return infoResult.Data;
                }
            } while (failed && tryCount <= maxTries);

            return null;
        }

        public async Task<Libs.Models.Details.Details> GetAnimeDetailsAsync(string seriesId)
        {
            const string infoApiCommand = "info.0.json";

            var request = new RestRequest(infoApiCommand, Method.GET);
            request.AddParameter("series_id", seriesId);
            return await this.ExecuteRequestAsync<Libs.Models.Details.Details>(request);
        }

        private async Task<string> GetSessionAsync()
        {
            foreach (var server in this.sessionServers)
            {
                var session = await server.GetSession();
                if (session != null && !session.Error)
                {
                    return session.Data.SessionId;
                }
            }

            return null;
        }
        
        
        public async Task<IEnumerable<Episode>> GetEpisodes(string collectionId)
        {
            const string listMediaUrlPart = "list_media.0.json";

                var request = new RestRequest(listMediaUrlPart, Method.GET);
                request.AddParameter("collection_id", collectionId);
                request.AddParameter("limit", int.MaxValue);

                return (IEnumerable<Episode>)await this.ExecuteRequestAsync<List<Episode>>(request) ?? Array.Empty<Episode>();
        }
    }
}