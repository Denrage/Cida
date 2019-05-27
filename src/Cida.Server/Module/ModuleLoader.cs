using System.Collections.Generic;
using System.IO;

namespace Cida.Server.Module
{
    public class ModuleLoaderManager
    {
        public const string ModuleFolderName = "Modules";
        public const string ModuleFileExtension = "cidam";
        private readonly string moduleDirectory;
        private readonly List<CidaModule> modules;
        
        public ModuleLoaderManager(string moduleDirectory)
        {
            this.moduleDirectory = moduleDirectory;
            this.modules = new List<CidaModule>();
        }

        public void LoadModules()
        {
            foreach (var modulePath in Directory.GetFiles(this.moduleDirectory, $"*.{ModuleFileExtension}"))
            {
                var module = CidaModule.Extract(modulePath);
                this.modules.Add(module);
                module.Load();
            }
        }
    }
}