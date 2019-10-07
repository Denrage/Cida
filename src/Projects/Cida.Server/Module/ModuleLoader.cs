using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cida.Server.Api;

namespace Cida.Server.Module
{
    public class ModuleLoaderManager
    {
        public const string ModuleFolderName = "Modules";
        public const string ModuleFileExtension = "cidam";
        private readonly string moduleDirectory;
        private readonly IGrpcRegistrar grpcRegistrar;
        private readonly List<CidaModule> modules;
        
        public ModuleLoaderManager(string moduleDirectory, IGrpcRegistrar grpcRegistrar)
        {
            this.moduleDirectory = moduleDirectory;
            this.grpcRegistrar = grpcRegistrar;
            this.modules = new List<CidaModule>();

            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }
        }

        public async Task LoadModulesAsync()
        {
            foreach (var modulePath in Directory.GetFiles(this.moduleDirectory, $"*.{ModuleFileExtension}"))
            {
                var module = CidaModule.Extract(modulePath);
                this.modules.Add(module);
                var loadedModule = module.Load();
                await this.grpcRegistrar.AddServiceAsync(loadedModule.GrpcService);
            }
        }
    }
}