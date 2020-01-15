using System;
using System.IO;
using System.Threading.Tasks;
using Cida.Server.Api;
using Cida.Server.Infrastructure;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.EFC;
using Cida.Server.Interfaces;
using Cida.Server.Module;
using NLog;

namespace Cida.Server
{
    public class CidaServer
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly ILogger logger;
        private readonly GrpcManager grpcManager;
        private readonly ModuleLoaderManager moduleLoader;
        private readonly InterNodeConnectionManager interNodeConnectionManager;

        public CidaServer(string workingDirectory, ISettingsProvider settingsProvider, ILogger logger)
        {
            this.settingsProvider = settingsProvider;
            this.logger = logger;
            this.grpcManager = new GrpcManager(settingsProvider.Get<GrpcConfiguration>(), this.logger);
            var globalConfigurationService = new GlobalConfigurationService(this.logger);
            var ftpClient = new FtpClient(globalConfigurationService, this.logger);
            var databaseProvider = new CidaDbConnectionProvider(globalConfigurationService);
            var cidaContext = new CidaContext(databaseProvider);
            this.moduleLoader = new ModuleLoaderManager(
                Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName),
                this.grpcManager,
                ftpClient,
                cidaContext,
                new DatabaseConnector(cidaContext, databaseProvider, globalConfigurationService),
                this.settingsProvider.Get<ServerConfiguration>().UnpackedModuleDirectories);
            globalConfigurationService.ConfigurationChanged += async () =>
            {
                try
                {
                    this.logger.Info("Saving configuration");
                    this.settingsProvider.Save(globalConfigurationService.Configuration);
                    this.logger.Info("Done saving configuration");
                    if (globalConfigurationService.ConfigurationManager?.Database?.Connection?.Host != null)
                    {
                        this.logger.Info("Ensure Database");
                        cidaContext.Database.EnsureCreated();
                        this.logger.Info("Database ensured");
                        this.logger.Info("Load Modules from database");
                        await this.moduleLoader.LoadFromDatabase();
                        this.logger.Info("Modules loaded");
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error(e);
                }
            };
            this.interNodeConnectionManager = new InterNodeConnectionManager(
                this.settingsProvider.Get<InfrastructureConfiguration>(),
                globalConfigurationService,
                ftpClient,
                databaseProvider,
                this.moduleLoader);

            globalConfigurationService.Update(configuration =>
            {
                var savedConfiguration = this.settingsProvider.Get<GlobalConfiguration>();
                configuration.Database = savedConfiguration.Database;
                configuration.Ftp = savedConfiguration.Ftp;
                configuration.Timestamp = savedConfiguration.Timestamp;
            }, false);

            this.interNodeConnectionManager.Start();

            Task.Run(async () =>
            {
                await this.grpcManager.AddServicesAsync(new[]
                    {Cida.CidaApiService.BindService(new GrpcManager.CidaApiService(this.moduleLoader))});
                await this.moduleLoader.LoadModulesAsync();
            });

        }
    }

    public class ServerConfiguration
    {
        public string[] UnpackedModuleDirectories { get; set; } = Array.Empty<string>();
    }
}