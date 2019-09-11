using System.IO;
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

            this.moduleLoader = new ModuleLoaderManager(Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName));
            this.moduleLoader.LoadModules();

            this.grpcManager = new GrpcManager(settingsProvider.Get<GrpcConfiguration>());
        }
    }
}