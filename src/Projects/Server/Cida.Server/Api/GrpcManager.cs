using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cida.Server.Module;
using Google.Protobuf;
using Grpc.Core;
using NLog;

namespace Cida.Server.Api
{
    public class GrpcManager : IGrpcRegistrar
    {
        private readonly ILogger logger;

        public class CidaApiService : Cida.CidaApiService.CidaApiServiceBase
        {
            private readonly ModuleLoaderManager moduleLoaderManager;
            private readonly ILogger logger;

            public CidaApiService(ModuleLoaderManager moduleLoaderManager, ILogger logger)
            {
                this.moduleLoaderManager = moduleLoaderManager;
                this.logger = logger;
            }

            public override Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
            {
                this.logger.Info($"Connected client {context.Peer}");
                return Task.FromResult(new VersionResponse()
                {
                    Version = 1,
                });
            }

            public override async Task<ClientModuleResponse> ClientModule(ClientModuleRequest request, ServerCallContext context)
            {
                this.logger.Info($"Client '{context.Peer}' requesting modules for Id '{request.Id}'");
                return new ClientModuleResponse()
                {
                    Streams = { (await this.moduleLoaderManager.GetClientModulesAsync(Guid.Parse(request.Id))).ToArray().Select(ByteString.CopyFrom) }
                };
            }
        }

        private readonly ServerPort[] ports;
        private readonly List<ServerServiceDefinition> services = new List<ServerServiceDefinition>();

        private Grpc.Core.Server grpcServer;

        public GrpcManager(IGrpcConfiguration configuration, ILogger logger)
        {
            this.logger = logger;
            this.ports = configuration.Endpoints.Select(x => new ServerPort(x.Host, x.Port, ServerCredentials.Insecure)).ToArray();
            this.grpcServer = this.CreateServer(this.services);
            logger.Info($"gRPC Server started on {configuration.Endpoints[0].Host}:{configuration.Endpoints[0].Port}");
        }

        public async Task Start()
        {
            this.grpcServer.Start();
            await Task.CompletedTask;
        }

        public async Task AddServicesAsync(IEnumerable<ServerServiceDefinition> definitions)
        {
            await this.grpcServer.ShutdownAsync();

            this.services.AddRange(definitions);

            this.grpcServer = this.CreateServer(this.services);

            this.grpcServer.Start();
        }

        public Grpc.Core.Server CreateServer(IEnumerable<ServerServiceDefinition> services)
        {
            var result = new Grpc.Core.Server(new[] { new ChannelOption(ChannelOptions.MaxSendMessageLength, -1), new ChannelOption(ChannelOptions.MaxReceiveMessageLength, -1) });

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
