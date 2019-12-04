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
            var globalConfigurationService = new GlobalConfigurationService(this.logger, this.settingsProvider.Get<GlobalConfiguration>());
            var ftpClient = new FtpClient(globalConfigurationService, this.logger);
            var databaseProvider = new CidaDbConnectionProvider(globalConfigurationService);
            var cidaContext = new CidaContext(databaseProvider);
            this.moduleLoader = new ModuleLoaderManager(
                Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName), 
                this.grpcManager,
                ftpClient,
                cidaContext,
                this.settingsProvider.Get<ServerConfiguration>().UnpackedModuleDirectories);
            globalConfigurationService.ConfigurationChanged += async () =>
            {
                this.logger.Info("Saving configuration");
                this.settingsProvider.Save(globalConfigurationService.Configuration);
                this.logger.Info("Done saving configuration");
                this.logger.Info("Ensure Database");
                await cidaContext.Database.EnsureCreatedAsync();
                this.logger.Info("Database ensured");
                this.logger.Info("Load Modules from database");
                await this.moduleLoader.LoadFromDatabase();
                this.logger.Info("Modules loaded");
            };
            this.interNodeConnectionManager = new InterNodeConnectionManager(
                this.settingsProvider.Get<InfrastructureConfiguration>(),
                globalConfigurationService,
                ftpClient,
                databaseProvider,
                this.moduleLoader);
            


            Task.Run(async () => await this.moduleLoader.LoadModulesAsync());
        }
    }
    
    public class ServerConfiguration
    {
        public string[] UnpackedModuleDirectories { get; set; } = Array.Empty<string>();
    }
}