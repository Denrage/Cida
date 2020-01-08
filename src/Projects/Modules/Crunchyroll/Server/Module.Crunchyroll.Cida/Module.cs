using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;
using Crunchyroll;
using Grpc.Core;
using Module.Crunchyroll.Cida.Extensions;
using Module.Crunchyroll.Cida.Services;

namespace Module.Crunchyroll.Cida
{
    public class Module : IModule
    {
        // Hack: Remove this asap
        private const string DatabasePassword = "Crunchyroll";
        private const string Id = "6640177C-1D1E-49D4-BA57-4A770F18AA7E";
        private string connectionString;
        private AnimeSearchCache cache;
        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(IDatabaseConnector databaseConnector)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);
            this.cache = new AnimeSearchCache(this.connectionString, new CrunchyrollApiService());
            this.GrpcServices = new[] {CrunchyrollService.BindService(new CrunchyRollImplementation(this.cache)), };
            Console.WriteLine("Loaded CR");
        }

        public class CrunchyRollImplementation : CrunchyrollService.CrunchyrollServiceBase
        {
            private readonly AnimeSearchCache cache;

            public CrunchyRollImplementation(AnimeSearchCache cache)
            {
                this.cache = cache;
            }

            public override async Task<SearchResponse> Search(SearchRequest request, ServerCallContext context) =>
                new SearchResponse()
                {
                    Items = { (await this.cache.SearchAsync(request.SearchTerm)).Select(x => x.ToGrpc()).ToArray() }
                };

            public override async Task<EpisodeResponse> GetEpisodes(EpisodeRequest request, ServerCallContext context)
            {
                return new EpisodeResponse()
                {
                    Episodes = { (await this.cache.GetEpisodesAsync(request.Id)).Select(x => x.ToGrpc()).ToArray() }
                    };
            }
        }
    }
}