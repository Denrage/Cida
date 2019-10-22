using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cida.Server.Models;
using Grpc.Core;
using Infrastructure;
using NLog;
using NLog.Fluent;

namespace Cida.Server.Infrastructure
{
    public class InterNodeConnectionManager
    {
        private readonly Grpc.Core.Server server;
        private readonly IList<Grpc.Core.Channel> connections;
        private readonly IInfrastructureConfiguration configuration;
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private CidaInfrastructureService.CidaInfrastructureServiceClient client;

        public InterNodeConnectionManager(IInfrastructureConfiguration configuration)
        {
            this.configuration = configuration;
            this.server = new Grpc.Core.Server();
            this.server.Ports.Add(this.configuration.ServerEndpoint.Host, this.configuration.ServerEndpoint.Port,
                ServerCredentials.Insecure);
            var implementation = new CidaInfrastructureServiceImplementation(this.logger);
            this.server.Services.Add(
                CidaInfrastructureService.BindService(implementation));

            implementation.OnSynchronize += ImplementationOnOnSynchronize;

            this.server.Start();
            this.logger.Info(
                $"InfrastructureServer started on {this.configuration.ServerEndpoint.Host}{this.configuration.ServerEndpoint.Port}");


            this.connections = new List<Channel>();

            if (!string.IsNullOrEmpty(configuration.Node.Host) && configuration.Node.Port != default)
            {
                this.InitializeClient(configuration.Node);
            }
        }

        private void ImplementationOnOnSynchronize(Endpoint endpoint)
        {
            this.InitializeClient(endpoint);
        }

        private void InitializeClient(Endpoint endpoint)
        {
            // TODO: Add algorithm to use more than one other connection
            if (this.connections.Count == 0 && this.client == null)
            {
                this.logger.Info($"Connecting to {endpoint.Host}:{endpoint.Port}");
                this.connections.Add(new Channel(endpoint.Host, endpoint.Port, ChannelCredentials.Insecure));
                this.client = new CidaInfrastructureService.CidaInfrastructureServiceClient(this.connections[0]);
                this.client.Synchronize(new SynchronizeRequest()
                {
                    PublicEndpoint = new SynchronizeRequest.Types.Endpoint()
                    {
                        Host = this.configuration.ServerEndpoint.Host,
                        Port = this.configuration.ServerEndpoint.Port,
                    },
                });
            }
        }

        public class CidaInfrastructureServiceImplementation : CidaInfrastructureService.CidaInfrastructureServiceBase
        {
            private readonly ILogger logger;
            public event Action<Endpoint> OnSynchronize;

            public CidaInfrastructureServiceImplementation(ILogger logger)
            {
                this.logger = logger;
            }
            
            public override async Task<SynchronizeResponse> Synchronize(SynchronizeRequest request,
                ServerCallContext context)
            {
                this.logger.Info("Got Synchronize");
                this.OnSynchronize?.Invoke(request.PublicEndpoint);
                return await Task.FromResult(new SynchronizeResponse());
            }
        }
    }

    public class InfrastructureConfiguration : IInfrastructureConfiguration
    {
        public Endpoint ServerEndpoint { get; set; } = new Endpoint();
     
        public Endpoint Node { get; set; } = new Endpoint();
    }

    public interface IInfrastructureConfiguration
    {
        Endpoint ServerEndpoint { get; }
        Endpoint Node { get; }
    }
}