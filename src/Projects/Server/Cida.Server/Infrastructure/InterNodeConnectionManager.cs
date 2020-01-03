using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cida.Server.Models;
using Cida.Server.Module;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Infrastructure;
using NLog;
using Cida.Server.Extensions;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.EFC;

namespace Cida.Server.Infrastructure
{
    public class InterNodeConnectionManager
    {
        private readonly Grpc.Core.Server server;
        private IList<Grpc.Core.Channel> connections;
        private readonly IInfrastructureConfiguration configuration;
        private readonly GlobalConfigurationService globalConfigurationService;
        private readonly IFtpClient ftpClient;
        private readonly CidaDbConnectionProvider provider;
        private readonly ModuleLoaderManager manager;
        private readonly IModulePublisher modulePublisher;
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private CidaInfrastructureService.CidaInfrastructureServiceClient client;

        // TODO: Better dependency injection
        public InterNodeConnectionManager(IInfrastructureConfiguration configuration,
            GlobalConfigurationService globalConfigurationService, IFtpClient ftpClient,
            CidaDbConnectionProvider provider, ModuleLoaderManager manager)
        {
            this.configuration = configuration;
            this.globalConfigurationService = globalConfigurationService;
            this.ftpClient = ftpClient;
            this.provider = provider;
            this.manager = manager;
            this.server = new Grpc.Core.Server();
            this.server.Ports.Add(this.configuration.ServerEndpoint.Host, this.configuration.ServerEndpoint.Port,
                ServerCredentials.Insecure);
            var implementation =
                new CidaInfrastructureServiceImplementation(this.logger, this.globalConfigurationService);
            this.server.Services.Add(
                CidaInfrastructureService.BindService(implementation));

            implementation.OnSynchronize += ImplementationOnOnSynchronize;

        }

        public void Start()
        {
            this.server.Start();
            this.logger.Info(
                $"InfrastructureServer started on {this.configuration.ServerEndpoint.Host}:{this.configuration.ServerEndpoint.Port}");


            this.connections = new List<Channel>();

            if (!string.IsNullOrEmpty(this.configuration.Node.Host) && this.configuration.Node.Port != default)
            {
                this.InitializeClient(this.configuration.Node);
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
                this.logger.Info("Connected. Sending synchronize.");
                var response = this.client.Synchronize(new SynchronizeRequest()
                {
                    PublicEndpoint = new SynchronizeRequest.Types.Endpoint()
                    {
                        Host = this.configuration.ServerEndpoint.Host,
                        Port = this.configuration.ServerEndpoint.Port,
                    },
                });

                // TODO: Set timestamp to received one
                if (response.Timestamp.ToDateTime() > this.globalConfigurationService.ConfigurationManager.Timestamp)
                {
                    this.logger.Info("Overwriting config because timestamp is newer");
                    this.globalConfigurationService.Update(config =>
                        {
                            config.Timestamp = response.Timestamp.ToDateTime();
                            config.Database = response.Database.FromGrpc();
                            config.Ftp = response.Ftp.FromGrpc();
                        }, false);
                }

                this.logger.Info("Synchronize successful");
            }
        }

        public class CidaInfrastructureServiceImplementation : CidaInfrastructureService.CidaInfrastructureServiceBase
        {
            private readonly ILogger logger;
            private readonly GlobalConfigurationService globalConfigurationService;
            public event Action<Endpoint> OnSynchronize;

            public CidaInfrastructureServiceImplementation(ILogger logger,
                GlobalConfigurationService globalConfigurationService)
            {
                this.logger = logger;
                this.globalConfigurationService = globalConfigurationService;
            }

            public override async Task<SynchronizeResponse> Synchronize(SynchronizeRequest request,
                ServerCallContext context)
            {
                this.logger.Info("Got Synchronize");
                this.OnSynchronize?.Invoke(request.PublicEndpoint);
                var result = new SynchronizeResponse()
                {
                    Timestamp = Timestamp.FromDateTime(this.globalConfigurationService.ConfigurationManager.Timestamp
                        .ToUniversalTime())
                };

                result.Ftp = this.globalConfigurationService.ConfigurationManager.Ftp.ToGrpc();
                result.Database = this.globalConfigurationService.ConfigurationManager.Database.ToGrpc();
                return await Task.FromResult(result);
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