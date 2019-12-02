using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cida.Server.Api;
using Cida.Server.Infrastructure;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;

namespace Cida.Server.Module
{
    public class ModuleLoaderManager : IModulePublisher
    {
        public const string ModuleFolderName = "Modules";
        public const string ModuleFileExtension = "cidam";
        private readonly string moduleDirectory;
        private readonly IEnumerable<string> unpackedModuleDirectories;
        private readonly IGrpcRegistrar grpcRegistrar;
        private readonly CidaDbConnectionProvider dbProvider;
        private readonly IFtpClient ftpClient;
        private readonly List<CidaModule> modules;

        public event Action ModulesUpdated;

        public IEnumerable<Guid> Modules
            => this.modules.Select(x => x.Metadata.Id);

        public ModuleLoaderManager(string moduleDirectory, IGrpcRegistrar grpcRegistrar,
            CidaDbConnectionProvider dbProvider, IFtpClient ftpClient,
            IEnumerable<string> unpackedModuleDirectories = null)
        {
            this.moduleDirectory = moduleDirectory;
            this.unpackedModuleDirectories = unpackedModuleDirectories ?? Array.Empty<string>();
            this.grpcRegistrar = grpcRegistrar;
            this.dbProvider = dbProvider;
            this.ftpClient = ftpClient;
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
                await this.grpcRegistrar.AddServicesAsync(loadedModule.GrpcServices);
            }

            foreach (var unpackedModuleDirectory in this.unpackedModuleDirectories)
            {
                var module = CidaModule.Unpacked(unpackedModuleDirectory);
                this.modules.Add(module);
                var loadedModule = module.Load();
                await this.grpcRegistrar.AddServicesAsync(loadedModule.GrpcServices);
            }

            // TODO: Move this to somewhere where it gets called everytime a module gets loaded
            // TODO: Maybe don't create context here
            var context = new CidaContext(this.dbProvider);
            var modulesInDatabase = context.FtpPaths.ToArray();
            var notUploaded = this.modules.Where(module =>
                modulesInDatabase.FirstOrDefault(x =>
                    Path.GetFileNameWithoutExtension(x.FtpPath) == module.Metadata.Id.ToString()) == null).ToList();

            // TODO: Add update mechanism
            foreach (var module in notUploaded)
            {
                byte[] zippedModule = Array.Empty<byte>();
                try
                {
                     zippedModule = await module.ToArchive();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                // TODO: Get path from somewhere else
                // TODO: Ensure all directories
                await this.ftpClient.SaveFileAsync(zippedModule, "Modules", module.Metadata.Id.ToString("N") + "." + ModuleFileExtension);
                context.Modules.Add(new ModuleInformation()
                {
                    ModuleId = module.Metadata.Id,
                    // TODO: Add name to metadata
                    ModuleName = module.Metadata.Id.ToString(),
                });
                context.FtpPaths.Add(new FtpInformation()
                {
                    // TODO: Get path from somewhere else
                    FtpPath = "Modules/" + module.Metadata.Id + "." + ModuleFileExtension,
                    ModuleId = module.Metadata.Id,
                });
                await context.SaveChangesAsync();
            }

            this.ModulesUpdated?.Invoke();
        }

        public async Task LoadModule(IEnumerable<byte> module)
        {
            var cidaModule = CidaModule.Extract(module.ToArray());
            this.modules.Add(cidaModule);
            var loadedModule = cidaModule.Load();
            await this.grpcRegistrar.AddServicesAsync(loadedModule.GrpcServices);
        }
    }

    public interface IModulePublisher
    {
        event Action ModulesUpdated;

        IEnumerable<Guid> Modules { get; }
    }
}