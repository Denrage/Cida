using System;
using System.IO;
using System.Threading.Tasks;
using Cida.Server.Api;
using Cida.Server.Infrastructure;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.Settings;
using Cida.Server.Interfaces;
using Cida.Server.Module;

namespace Cida.Server
{
    public class CidaServer
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly GrpcManager grpcManager;
        private readonly ModuleLoaderManager moduleLoader;
        private readonly InterNodeConnectionManager interNodeConnectionManager;

        public CidaServer(string workingDirectory, ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
            var databaseProvider = new CidaDbConnectionProvider(new MockSettingsManager());
            var ftpClient = new FtpClient(this.settingsProvider);
            this.grpcManager = new GrpcManager(settingsProvider.Get<GrpcConfiguration>());
            this.moduleLoader = new ModuleLoaderManager(
                Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName), 
                this.grpcManager,
                databaseProvider,
                ftpClient,
                this.settingsProvider.Get<ServerConfiguration>().UnpackedModuleDirectories);
            var globalConfigurationService = new GlobalConfigurationService(this.moduleLoader, this.settingsProvider.Get<GlobalConfigurationService.GlobalConfiguration>());
            globalConfigurationService.ConfigurationChanged += () =>
            {
                this.settingsProvider.Save(globalConfigurationService.Configuration);
            };
            this.interNodeConnectionManager = new Infrastructure.InterNodeConnectionManager(
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