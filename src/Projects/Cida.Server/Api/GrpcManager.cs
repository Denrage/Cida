using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ServerPort[] ports;
        private readonly List<ServerServiceDefinition> services = new List<ServerServiceDefinition>();

        private Grpc.Core.Server grpcServer;

        public GrpcManager(IGrpcConfiguration configuration)
        {
            this.ports = configuration.Endpoints.Select(x => new ServerPort(x.Endpoint, x.Port, ServerCredentials.Insecure)).ToArray();
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

            foreach (var serverPort in this.ports)
            {
                result.Ports.Add(serverPort);
            }

            foreach (var service in services)
            {
                result.Services.Add(service);
            }

            return result;
        }
    }
}
