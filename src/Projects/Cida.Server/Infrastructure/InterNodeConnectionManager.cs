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
        private readonly IList<Grpc.Core.Channel> connections;
        private readonly IInfrastructureConfiguration configuration;
        private readonly GlobalConfigurationService globalConfigurationService;
        private readonly IFtpClient ftpClient;
        private readonly CidaDbConnectionProvider provider;
        private readonly ModuleLoaderManager manager;
        private readonly IModulePublisher modulePublisher;
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private CidaInfrastructureService.CidaInfrastructureServiceClient client;

        public InterNodeConnectionManager(IInfrastructureConfiguration configuration, GlobalConfigurationService globalConfigurationService, IFtpClient ftpClient, CidaDbConnectionProvider provider, ModuleLoaderManager manager)
        {
            this.configuration = configuration;
            this.globalConfigurationService = globalConfigurationService;
            this.ftpClient = ftpClient;
            this.provider = provider;
            this.manager = manager;
            this.server = new Grpc.Core.Server();
            this.server.Ports.Add(this.configuration.ServerEndpoint.Host, this.configuration.ServerEndpoint.Port,
                ServerCredentials.Insecure);
            var implementation = new CidaInfrastructureServiceImplementation(this.logger, this.globalConfigurationService);
            this.server.Services.Add(
                CidaInfrastructureService.BindService(implementation));

            implementation.OnSynchronize += ImplementationOnOnSynchronize;
            this.server.Start();
            this.logger.Info(
                $"InfrastructureServer started on {this.configuration.ServerEndpoint.Host}:{this.configuration.ServerEndpoint.Port}");


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
                this.logger.Info("Connected. Sending synchronize.");
                var response = this.client.Synchronize(new SynchronizeRequest()
                {
                    PublicEndpoint = new SynchronizeRequest.Types.Endpoint()
                    {
                        Host = this.configuration.ServerEndpoint.Host,
                        Port = this.configuration.ServerEndpoint.Port,
                    },
                });

                var databaseContext = new CidaContext(this.provider);
                this.DownloadMissingModules(databaseContext.FtpPaths.Where(x => response.Modules.Contains(x.ModuleId.ToString())).Select(x => x.FtpPath));
                this.logger.Info("Synchronize successful");
            }
        }

        private void DownloadMissingModules(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var file = this.ftpClient.GetFileAsync(path).GetAwaiter().GetResult();
                this.manager.LoadModule(file).GetAwaiter().GetResult();
            }
        }
        
        public class CidaInfrastructureServiceImplementation : CidaInfrastructureService.CidaInfrastructureServiceBase
        {
            private readonly ILogger logger;
            private readonly GlobalConfigurationService globalConfigurationService;
            public event Action<Endpoint> OnSynchronize;

            public CidaInfrastructureServiceImplementation(ILogger logger, GlobalConfigurationService globalConfigurationService)
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
                    Timestamp = Timestamp.FromDateTime(this.globalConfigurationService.Configuration.Timestamp.ToUniversalTime())
                };

                if (this.globalConfigurationService.Configuration.Modules?.Any() ?? false)
                {
                    result.Modules.AddRange(this.globalConfigurationService.Configuration.Modules.Select(x => x.ToString()));
                }

                result.Ftp = this.globalConfigurationService.Configuration.Ftp.ToGrpc();
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