﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;

namespace Cida.Server.Api
{
    public class GrpcManager : IGrpcRegistrar
    {
        private readonly ILogger logger;

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

        public GrpcManager(IGrpcConfiguration configuration, ILogger logger)
        {
            this.logger = logger;
            this.ports = configuration.Endpoints.Select(x => new ServerPort(x.Host, x.Port, ServerCredentials.Insecure)).ToArray();
            this.services.Add(Cida.CidaApiService.BindService(new CidaApiService()));
            this.grpcServer = this.CreateServer(this.services);
            this.grpcServer.Start();
            logger.Info($"gRPC Server started on {configuration.Endpoints[0].Host}:{configuration.Endpoints[0].Port}");
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
