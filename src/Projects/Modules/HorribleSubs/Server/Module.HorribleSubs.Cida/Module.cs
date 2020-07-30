using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;
using Google.Protobuf.Collections;
using Grpc.Core;
using Horriblesubs;
using Module.Crunchyroll.Cida.Extensions;
using Module.HorribleSubs.Cida.Services;

namespace Module.Crunchyroll.Cida
{
    public class Module : IModule
    {
        // Hack: Remove this asap
        private const string DatabasePassword = "HorribleSubs";
        private const string Id = "109F8F54-DE94-479A-840A-7B4EF0F284D2";
        private string connectionString;
        private SearchService searchService;

        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public async Task Load(IDatabaseConnector databaseConnector, IFtpClient ftpClient)
        {
            this.connectionString =
                await databaseConnector.GetDatabaseConnectionStringAsync(Guid.Parse(Id), DatabasePassword);

            this.searchService = new SearchService();

            this.GrpcServices = new[] {HorribleSubsService.BindService(new HorribleSubsImplementation(this.searchService)), };
            Console.WriteLine("Loaded CR");
        }

        public class HorribleSubsImplementation : HorribleSubsService.HorribleSubsServiceBase
        {
            private readonly SearchService searchService;

            public HorribleSubsImplementation(SearchService searchService)
            {
                this.searchService = searchService;
            }

            public override async Task<SearchResponse> Search(SearchRequest request, ServerCallContext context)
            {
                return new SearchResponse()
                {
                    SearchResults = { (await this.searchService.SearchAsync(request.SearchTerm)).Select(x => x.ToGrpc()).ToArray() }
                };
            }
        }
    }
}