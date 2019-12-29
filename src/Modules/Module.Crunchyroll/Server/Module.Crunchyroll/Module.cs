using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;
using Crunchyroll;
using Grpc.Core;
using Module.Crunchyroll.Extensions;

namespace Module.Crunchyroll
{
    public class Module : IModule
    {
        private AnimeSearchCache cache;
        public IEnumerable<ServerServiceDefinition> GrpcServices { get; private set; } = Array.Empty<ServerServiceDefinition>();

        public void Load()
        {
            this.cache = new AnimeSearchCache();
            this.GrpcServices = new[] {CrunchyrollService.BindService(new CrunchyRollImplementation(this.cache)),};
            Console.WriteLine("Loaded CR");
        }

        public class CrunchyRollImplementation : CrunchyrollService.CrunchyrollServiceBase
        {
            private readonly AnimeSearchCache cache;

            public CrunchyRollImplementation(AnimeSearchCache cache)
            {
                this.cache = cache;
            }

            public override Task<SearchResponse> Search(SearchRequest request, ServerCallContext context)
            {
                return Task.FromResult(new SearchResponse()
                {
                    Items = {this.cache.Search(request.SearchTerm).Select(x => x.ToGrpc())}
                });
            }
        }
    }
}