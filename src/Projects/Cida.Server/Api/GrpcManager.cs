using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Cida.Server.Api
{
    public class GrpcManager
    {
        public class CidaApiService : Cida.CidaApiService.CidaApiServiceBase
        {
            public override Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
            {
                return Task.FromResult(new VersionResponse()
                {
                    Version = 1,
                });
            }
        }

        private readonly ServerPort defaultPort = new ServerPort("localhost", 50051, ServerCredentials.Insecure);
        private readonly List<ServerServiceDefinition> services = new List<ServerServiceDefinition>();

        private Grpc.Core.Server grpcServer;

        public GrpcManager()
        {
            this.grpcServer = this.CreateServer(Array.Empty<ServerServiceDefinition>());

            this.grpcServer.Start();
        }

        public async Task AddService()
        {
            await this.grpcServer.ShutdownAsync();

            this.services.Add(Cida.CidaApiService.BindService(new CidaApiService()));

            this.grpcServer = this.CreateServer(this.services);

            this.grpcServer.Start();
        }

        public Grpc.Core.Server CreateServer(IEnumerable<ServerServiceDefinition> services)
        {
            var result = new Grpc.Core.Server();
            result.Ports.Add(defaultPort);

            foreach (var service in services)
            {
                result.Services.Add(service);
            }

            return result;
        }
    }
}
