using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;
using Cida.Server.Api;
using Cida.Server.Extensions;
using Cida.Server.Infrastructure;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NLog;

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
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<Guid, CidaModule> modules;
        private readonly ConcurrentBag<Guid> unpackedModules;

        public event Action ModulesUpdated;

        public IEnumerable<Guid> Modules
            => this.modules.Select(x => x.Key);

        public ModuleLoaderManager(string moduleDirectory, IGrpcRegistrar grpcRegistrar, IFtpClient ftpClient,
            CidaContext databaseContext, IDatabaseConnector databaseConnector, ILogger logger,
            IEnumerable<string> unpackedModuleDirectories = null)
        {
            this.moduleDirectory = moduleDirectory;
            this.unpackedModuleDirectories = unpackedModuleDirectories ?? Array.Empty<string>();
            this.grpcRegistrar = grpcRegistrar;
            this.ftpClient = ftpClient;
            this.databaseContext = databaseContext;
            this.databaseConnector = databaseConnector;
            this.logger = logger;
            this.modules = new ConcurrentDictionary<Guid, CidaModule>();
            this.unpackedModules = new ConcurrentBag<Guid>();

            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }
        }

        public async Task<IModule> LoadUnpacked(string unpackedModule)
        {
            CidaModule module = null;
            this.logger.Info($"Loading unpacked module '{unpackedModule}'");
            try
            {
                module = CidaModule.Unpacked(unpackedModule);
                if (await this.databaseContext.Modules
                    .AnyAsync(x => x.ModuleId == module.Metadata.Id))
                {
                    await this.databaseContext.Modules.AddAsync(new ModuleInformation()
                    {
                        ModuleId = module.Metadata.Id,
                        ModuleName = module.Metadata.Name,
                    });

                    await this.databaseContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error parsing module '{unpackedModule}'");
            }

            if (module != null && this.modules.TryAdd(module.Metadata.Id, module))
            {
                this.unpackedModules.Add(module.Metadata.Id);
                var loadedModule = await this.LoadModule(module);
                this.logger.Info("Done loading unpacked module");
                return loadedModule;
            }

            return null;
        }

        public async Task<IModule> LoadPacked(IEnumerable<byte> bytes)
        {
            CidaModule module = null;
            this.logger.Info($"Loading packed module from Database");
            try
            {
                module = CidaModule.Extract(bytes.ToArray());
                if (await this.databaseContext.Modules
                    .AnyAsync(x => x.ModuleId == module.Metadata.Id))
                {
                    await this.databaseContext.Modules.AddAsync(new ModuleInformation()
                    {
                        ModuleId = module.Metadata.Id,
                        ModuleName = module.Metadata.Name,
                    });

                    await this.databaseContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error parsing module");
            }

            if (module != null && this.modules.TryAdd(module.Metadata.Id, module))
            {
                var loadedModule = await this.LoadModule(module);
                this.logger.Info("Done loading packed module");
                return loadedModule;
            }

            return null;
        }

        public async Task<IModule> LoadPacked(string file)
        {
            CidaModule module = null;
            this.logger.Info($"Loading packed module '{file}'");
            try
            {
                module = CidaModule.Extract(file);
                if (await this.databaseContext.Modules
                    .AnyAsync(x => x.ModuleId == module.Metadata.Id))
                {
                    await this.databaseContext.Modules.AddAsync(new ModuleInformation()
                    {
                        ModuleId = module.Metadata.Id,
                        ModuleName = module.Metadata.Name,
                    });

                    await this.databaseContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error parsing module '{file}'");
            }

            if (module != null && this.modules.TryAdd(module.Metadata.Id, module))
            {
                var loadedModule = await this.LoadModule(module);
                this.logger.Info("Done loading packed module");
                return loadedModule;
            }

            return null;
        }


        public async Task Start()
        {
            this.logger.Info("Loading modules");
            var services = new List<ServerServiceDefinition>();
            this.logger.Info("Loading unpacked modules");

            foreach (var unpackedModule in this.unpackedModuleDirectories)
            {
                var module = await this.LoadUnpacked(unpackedModule);
                if (module != null)
                {
                    services.AddRange(module.GrpcServices);
                }
            }

            this.logger.Info("Done loading unpacked modules");
            this.logger.Info("Loading packed modules");
            foreach (var modulePath in Directory.GetFiles(this.moduleDirectory, $"*.{ModuleFileExtension}"))
            {
                var module = await this.LoadPacked(modulePath);
                if (module != null)
                {
                    services.AddRange(module.GrpcServices);
                }
            }

            await this.grpcRegistrar.AddServicesAsync(services);

            this.logger.Info("Done loading packed modules");

            await this.UploadToDatabase();

            await this.LoadFromDatabase();

            this.logger.Info("Done loading modules");
        }

        private async Task LoadFromDatabase()
        {
            this.logger.Info("Loading Database modules");
            var services = new List<ServerServiceDefinition>();
            // TODO: Save locally
            var modulePaths = await this.databaseContext.FtpPaths.ToArrayAsync();
            foreach (var path in modulePaths.Where(x => !this.Modules.Contains(x.ModuleId)).Select(x => x.FtpPath))
            {
                var file = await this.ftpClient.GetFileAsync(path);
                var loadedModule = await this.LoadPacked(file);
                if (loadedModule != null)
                {
                    services.AddRange(loadedModule.GrpcServices);
                }
            }

            await this.grpcRegistrar.AddServicesAsync(services);
            this.logger.Info("Done loading Database modules");
        }

        private async Task UploadToDatabase()
        {
            this.logger.Info("Upload modules");
            // TODO: Move this to somewhere where it gets called everytime a module gets loaded
            var modulesInDatabase = await this.databaseContext.FtpPaths.ToArrayAsync();
            var notUploaded = this.modules.Where(module =>
                    modulesInDatabase.FirstOrDefault(x =>
                        Path.GetFileNameWithoutExtension(x.FtpPath) == module.Value.Metadata.IdToString()) ==
                    null)
                .ToList();

            // TODO: Add update mechanism
            foreach (var module in notUploaded
                .Where(x => !this.unpackedModules.Contains(x.Key)))
            {
                this.logger.Info($"Uploading module {module.Value.Metadata.Name}({module.Value.Metadata.Id})");
                var zippedModule = await module.Value.ToArchive();

                try
                {
                    // TODO: Ensure all directories
                    await this.ftpClient.SaveFileAsync(zippedModule, "Modules",
                        $"{module.Value.Metadata.IdToString()}.{ModuleFileExtension}");


                    await this.databaseContext.FtpPaths.AddAsync(new FtpInformation()
                    {
                        // TODO: Get path from somewhere else
                        FtpPath = "Modules/" + module.Value.Metadata.Id.ToString("N") + "." + ModuleFileExtension,
                        ModuleId = module.Value.Metadata.Id,
                    });

                    await this.databaseContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Couldn't upload module");
                }

                this.logger.Info($"Done uploading module");
            }
        }

        private async Task<IModule> LoadModule(CidaModule module)
        {
            try
            {
                if (this.modules.TryAdd(module.Metadata.Id, module))
                {
                    return await module.Load(this.databaseConnector);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Could not load module {module.Metadata.Name}({module.Metadata.Id})");
            }

            return null;
        }
        public async Task<IEnumerable<byte[]>> GetClientModulesAsync(Guid id)
        {
            return (await Task.WhenAll(this.modules.Select(x => x.Value.GetClientModule(id)))).Where(x => x.Length > 0);
        }
    }

    public interface IModulePublisher
    {
        event Action ModulesUpdated;

        IEnumerable<Guid> Modules { get; }
    }
}