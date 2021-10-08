using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cida.Api;
using Cida.Server.Api;
using Cida.Server.Extensions;
using Cida.Server.Infrastructure;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.Models;
using Cida.Server.Interfaces;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Cida.Server.Module
{
    public class ModuleLoaderManager
    {
        public const string ModuleFolderName = "Modules";
        public const string ModuleFileExtension = "cidam";

        private readonly Cida.Api.Models.Filesystem.Directory ModuleDirectory = new Cida.Api.Models.Filesystem.Directory(ModuleFolderName, null);
        private readonly string moduleDirectory;
        private readonly IEnumerable<string> unpackedModuleDirectories;
        private readonly IGrpcRegistrar grpcRegistrar;
        private readonly Infrastructure.IFtpClient ftpClient;
        private readonly Func<CidaContext> getDatabaseContext;
        private readonly IDatabaseConnector databaseConnector;
        private readonly ILogger logger;
        private readonly GlobalConfigurationService globalConfigurationService;
        private readonly IModuleFtpClientFactory moduleFtpClientFactory;
        private readonly IModuleLoggerFactory moduleLoggerFactory;
        private readonly ConcurrentDictionary<Guid, CidaModule> modules;
        private readonly ConcurrentBag<Guid> unpackedModules;

        public IEnumerable<Guid> Modules
            => this.modules.Select(x => x.Key);

        public ModuleLoaderManager(
            string moduleDirectory,
            IGrpcRegistrar grpcRegistrar,
            Infrastructure.IFtpClient ftpClient,
            Func<CidaContext> getDatabaseContext,
            IDatabaseConnector databaseConnector,
            ILogger logger,
            GlobalConfigurationService globalConfigurationService,
            IModuleFtpClientFactory moduleFtpClientFactory,
            IModuleLoggerFactory moduleLoggerFactory,
            IEnumerable<string>? unpackedModuleDirectories = null)
        {
            this.moduleDirectory = moduleDirectory;
            this.unpackedModuleDirectories = unpackedModuleDirectories ?? Array.Empty<string>();
            this.grpcRegistrar = grpcRegistrar;
            this.ftpClient = ftpClient;
            this.getDatabaseContext = getDatabaseContext;
            this.databaseConnector = databaseConnector;
            this.logger = logger;
            this.globalConfigurationService = globalConfigurationService;
            this.moduleFtpClientFactory = moduleFtpClientFactory;
            this.moduleLoggerFactory = moduleLoggerFactory;
            this.modules = new ConcurrentDictionary<Guid, CidaModule>();
            this.unpackedModules = new ConcurrentBag<Guid>();

            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }
        }

        public async Task<IModule?> LoadUnpacked(string unpackedModule)
        {
            CidaModule? module = null;
            this.logger.Info($"Loading unpacked module '{unpackedModule}'");
            try
            {
                module = CidaModule.Unpacked(unpackedModule);
                using (var context = this.getDatabaseContext())
                {
                    if (!await context.Modules
                        .AnyAsync(x => x.ModuleId == module.Metadata.Id))
                    {
                        await context.Modules.AddAsync(new ModuleInformation()
                        {
                            ModuleId = module.Metadata.Id,
                            ModuleName = module.Metadata.Name,
                        });

                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error parsing module '{unpackedModule}'");
            }

            if (module != null)
            {
                this.unpackedModules.Add(module.Metadata.Id);
                var loadedModule = await this.LoadModule(module);
                this.logger.Info("Done loading unpacked module");
                return loadedModule;
            }

            return null;
        }

        public async Task<IModule?> LoadPacked(IEnumerable<byte> bytes)
        {
            CidaModule? module = null;
            this.logger.Info($"Loading packed module from Database");
            try
            {
                module = CidaModule.Extract(bytes.ToArray());
                using (var context = this.getDatabaseContext())
                {
                    if (!await context.Modules
                        .AnyAsync(x => x.ModuleId == module.Metadata.Id))
                    {
                        await context.Modules.AddAsync(new ModuleInformation()
                        {
                            ModuleId = module.Metadata.Id,
                            ModuleName = module.Metadata.Name,
                        });

                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error parsing module");
            }

            if (module != null)
            {
                var loadedModule = await this.LoadModule(module);
                this.logger.Info("Done loading packed module");
                return loadedModule;
            }

            return null;
        }

        public async Task<IModule?> LoadPacked(string file)
        {
            CidaModule? module = null;
            this.logger.Info($"Loading packed module '{file}'");
            try
            {
                module = CidaModule.Extract(file);
                using (var context = this.getDatabaseContext())
                {
                    if (!await context.Modules
                        .AnyAsync(x => x.ModuleId == module.Metadata.Id))
                    {
                        await context.Modules.AddAsync(new ModuleInformation()
                        {
                            ModuleId = module.Metadata.Id,
                            ModuleName = module.Metadata.Name,
                        });

                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error parsing module '{file}'");
            }

            if (module != null)
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

            await this.UploadToDatabase(default);

            await this.LoadFromDatabase(default);

            this.logger.Info("Done loading modules");

            this.globalConfigurationService.ConfigurationChanged += async () =>
            {
                this.logger.Info("Loading modules from new database configuration");
                await this.UploadToDatabase(default);
                await this.LoadFromDatabase(default);
                this.logger.Info("Done loading modules from new database configuration");
            };
        }

        private async Task LoadFromDatabase(CancellationToken cancellationToken)
        {
            this.logger.Info("Loading Database modules");
            var services = new List<ServerServiceDefinition>();
            // TODO: Save locally

            using var context = this.getDatabaseContext();

            var modulePaths = await context.FtpPaths.ToArrayAsync(cancellationToken);
            foreach (var path in modulePaths.Where(x => !this.Modules.Contains(x.ModuleId)).Select(x => x.FtpPath))
            {
                using var requestedFile = new Cida.Api.Models.Filesystem.File(Path.GetFileName(path), this.ModuleDirectory, null);
                using var file = await this.ftpClient.GetFileAsync(requestedFile, cancellationToken);

                if(file.Equals(Cida.Api.Models.Filesystem.File.EmptyFile))
                {
                    this.logger.Warn($"Error occured on downloading module file: '{path}'");
                    continue;
                }

                await using var stream = await file.GetStreamAsync(cancellationToken);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken);
                var loadedModule = await this.LoadPacked(memoryStream.ToArray());
                if (loadedModule != null)
                {
                    services.AddRange(loadedModule.GrpcServices);
                }
            }

            await this.grpcRegistrar.AddServicesAsync(services);
            this.logger.Info("Done loading Database modules");
        }

        private async Task UploadToDatabase(CancellationToken cancellationToken)
        {
            this.logger.Info("Upload modules");
            // TODO: Move this to somewhere where it gets called everytime a module gets loaded
            using var context = this.getDatabaseContext();
            var modulesInDatabase = await context.FtpPaths.ToArrayAsync(cancellationToken);
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

                async Task<Stream> getStream(CancellationToken cancellationToken)
                    => await module.Value.ToArchiveStream();

                try
                {
                    using var file = new Cida.Api.Models.Filesystem.File($"{module.Value.Metadata.IdToString()}.{ModuleFileExtension}", this.ModuleDirectory, getStream);
                    await this.ftpClient.SaveFileAsync(file, cancellationToken);

                    await context.FtpPaths.AddAsync(new FtpInformation()
                    {
                        FtpPath = file.FullPath("/"),
                        ModuleId = module.Value.Metadata.Id,
                    }, cancellationToken);

                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Couldn't upload module");
                }

                this.logger.Info($"Done uploading module");
            }
        }

        private async Task<IModule?> LoadModule(CidaModule module)
        {
            try
            {
                if (this.modules.TryAdd(module.Metadata.Id, module))
                {
                    var rootDirectory = new Cida.Api.Models.Filesystem.Directory("ModuleFiles", null);
                    var moduleDirectory = new Cida.Api.Models.Filesystem.Directory(module.Metadata.Id.ToString(), rootDirectory);
                    return await module.Load(this.databaseConnector, this.moduleFtpClientFactory.Create(moduleDirectory), moduleDirectory, this.moduleLoggerFactory.Create(module.Metadata.Name));
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
}
