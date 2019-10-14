using System;
using System.IO;
using System.Threading.Tasks;
using Cida.Server.Api;
using Cida.Server.Interfaces;
using Cida.Server.Module;

namespace Cida.Server
{
    public class CidaServer
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly GrpcManager grpcManager;
        private readonly ModuleLoaderManager moduleLoader;

        public CidaServer(string workingDirectory, ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
            this.grpcManager = new GrpcManager(settingsProvider.Get<GrpcConfiguration>());

            this.moduleLoader = new ModuleLoaderManager(
                Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName), 
                this.grpcManager, 
                this.settingsProvider.Get<ServerConfiguration>().UnpackedModuleDirectories);
            Task.Run(async () => await this.moduleLoader.LoadModulesAsync());
        }
    }
    
    public class ServerConfiguration
    {
        public string[] UnpackedModuleDirectories { get; set; } = Array.Empty<string>();
    }
}