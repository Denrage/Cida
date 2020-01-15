using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;
using Cida.Server.Api;
using Cida.Server.Infrastructure;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Grpc.Core;

namespace Cida.Server.Module
{
    public class ModuleLoaderManager : IModulePublisher
    {
        public const string ModuleFolderName = "Modules";
        public const string ModuleFileExtension = "cidam";
        private readonly string moduleDirectory;
        private readonly IEnumerable<string> unpackedModuleDirectories;
        private readonly IGrpcRegistrar grpcRegistrar;
        private readonly IFtpClient ftpClient;
        private readonly CidaContext databaseContext;
        private readonly IDatabaseConnector databaseConnector;
        private readonly ConcurrentDictionary<Guid, CidaModule> modules;

        public event Action ModulesUpdated;

        public IEnumerable<Guid> Modules
            => this.modules.Select(x => x.Key);

        public ModuleLoaderManager(string moduleDirectory, IGrpcRegistrar grpcRegistrar, IFtpClient ftpClient,
            CidaContext databaseContext, IDatabaseConnector databaseConnector,
            IEnumerable<string> unpackedModuleDirectories = null)
        {
            this.moduleDirectory = moduleDirectory;
            this.unpackedModuleDirectories = unpackedModuleDirectories ?? Array.Empty<string>();
            this.grpcRegistrar = grpcRegistrar;
            this.ftpClient = ftpClient;
            this.databaseContext = databaseContext;
            this.databaseConnector = databaseConnector;
            this.modules = new ConcurrentDictionary<Guid, CidaModule>();

            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }
        }

        public async Task LoadFromDatabase()
        {
            // TODO: Save locally
            var modulePaths = this.databaseContext.FtpPaths.ToArray();
            foreach (var path in modulePaths.Where(x => !this.Modules.Contains(x.ModuleId)).Select(x => x.FtpPath))
            {
                var file = await this.ftpClient.GetFileAsync(path);
                await this.LoadModule(file);
            }
        }

        public async Task LoadModulesAsync()
        {
            try
            {
                var services = new List<ServerServiceDefinition>();
                foreach (var modulePath in Directory.GetFiles(this.moduleDirectory, $"*.{ModuleFileExtension}"))
                {
                    var module = CidaModule.Extract(modulePath);
                    if (this.modules.TryAdd(module.Metadata.Id, module))
                    {
                        var loadedModule = await module.Load(this.databaseConnector);
                        services.AddRange(loadedModule.GrpcServices);
                    }
                }

                foreach (var unpackedModuleDirectory in this.unpackedModuleDirectories)
                {
                    var module = CidaModule.Unpacked(unpackedModuleDirectory);
                    if (this.modules.TryAdd(module.Metadata.Id, module))
                    {
                        // this.databaseContext.Modules.Add(new ModuleInformation()
                        // {
                        //     ModuleId = module.Metadata.Id,
                        //     ModuleName = module.Metadata.Name,
                        // });
                        // await this.databaseContext.SaveChangesAsync();
                        var loadedModule = await module.Load(this.databaseConnector);
                        services.AddRange(loadedModule.GrpcServices);
                    }
                }

                // TODO: Move this to somewhere where it gets called everytime a module gets loaded
                 // var modulesInDatabase = this.databaseContext.FtpPaths.ToArray();
                 // var notUploaded = this.modules.Where(module =>
                 //         modulesInDatabase.FirstOrDefault(x =>
                 //             Path.GetFileNameWithoutExtension(x.FtpPath) == module.Value.Metadata.Id.ToString("N")) ==
                 //         null)
                 //     .ToList();
                //
                // // TODO: Add update mechanism
                 // foreach (var module in notUploaded)
                 // {
                //     var zippedModule = await module.Value.ToArchive();
                //
                //     // TODO: Get path from somewhere else
                //     // TODO: Ensure all directories
                //     try
                //     {
                //         await this.ftpClient.SaveFileAsync(zippedModule, "Modules",
                //             module.Value.Metadata.Id.ToString("N") + "." + ModuleFileExtension);
                //     }
                //     catch (Exception ex)
                //     {
                //         throw;
                //     }
                //
                     // this.databaseContext.Modules.Add(new ModuleInformation()
                     // {
                     //     ModuleId = module.Value.Metadata.Id,
                     //     ModuleName = module.Value.Metadata.Name,
                     // });
                //     this.databaseContext.FtpPaths.Add(new FtpInformation()
                //     {
                //         // TODO: Get path from somewhere else
                //         FtpPath = "Modules/" + module.Value.Metadata.Id.ToString("N") + "." + ModuleFileExtension,
                //         ModuleId = module.Value.Metadata.Id,
                //     });
                 //     await this.databaseContext.SaveChangesAsync();
                 // }

                await this.grpcRegistrar.AddServicesAsync(services);

                this.ModulesUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task<IEnumerable<byte[]>> GetClientModulesAsync(Guid id)
        {
            return (await Task.WhenAll(this.modules.Select(x => x.Value.GetClientModule(id)))).Where(x => x.Length > 0);
        }

        public async Task LoadModule(IEnumerable<byte> module)
        {
            var cidaModule = CidaModule.Extract(module.ToArray());
            if (this.modules.TryAdd(cidaModule.Metadata.Id, cidaModule))
            {
                var loadedModule = await cidaModule.Load(this.databaseConnector);
                await this.grpcRegistrar.AddServicesAsync(loadedModule.GrpcServices);
            }
        }
    }

    public interface IModulePublisher
    {
        event Action ModulesUpdated;

        IEnumerable<Guid> Modules { get; }
    }
}