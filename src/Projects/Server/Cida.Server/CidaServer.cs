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
        private GlobalConfigurationService globalConfigurationService;
        private FtpClient ftpClient;
        private CidaDbConnectionProvider databaseProvider;
        private CidaContext cidaContext;

        public CidaServer(string workingDirectory, ISettingsProvider settingsProvider, ILogger logger)
        {
            this.settingsProvider = settingsProvider;
            this.logger = logger;
            this.grpcManager = new GrpcManager(settingsProvider.Get<GrpcConfiguration>(), this.logger);
            this.globalConfigurationService = new GlobalConfigurationService(this.logger);
            this.ftpClient = new FtpClient(globalConfigurationService, this.logger);
            this.databaseProvider = new CidaDbConnectionProvider(globalConfigurationService);
            this.cidaContext = new CidaContext(databaseProvider);
            this.moduleLoader = new ModuleLoaderManager(
                Path.Combine(workingDirectory, ModuleLoaderManager.ModuleFolderName),
                this.grpcManager,
                ftpClient,
                cidaContext,
                new DatabaseConnector(cidaContext, databaseProvider, globalConfigurationService),
                this.logger,
                this.globalConfigurationService,
                this.settingsProvider.Get<ServerConfiguration>().UnpackedModuleDirectories);

            this.interNodeConnectionManager = new InterNodeConnectionManager(
                this.settingsProvider.Get<InfrastructureConfiguration>(),
                globalConfigurationService,
                ftpClient,
                databaseProvider,
                this.moduleLoader);
        }

        public async Task Start()
        {
            globalConfigurationService.ConfigurationChanged += async () =>
            {
                try
                {
                    this.logger.Info("Saving configuration");
                    this.settingsProvider.Save(globalConfigurationService.Configuration);
                    this.logger.Info("Done saving configuration");
                    if (globalConfigurationService.ConfigurationManager?.Database?.Connection?.Host != null)
                    {
                        //TODO: Move this somewhere else
                        this.logger.Info("Ensure Database");
                        cidaContext.Database.EnsureCreated();
                        this.logger.Info("Database ensured");
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error(e);
                }
            };
    
            await this.grpcManager.Start();
            await this.interNodeConnectionManager.Start();

            globalConfigurationService.Update(configuration =>
            {
                var savedConfiguration = this.settingsProvider.Get<GlobalConfiguration>();
                
                this.ValidateDatabase(savedConfiguration.Database);
                this.ValidateFtp(savedConfiguration.Ftp);
                
                configuration.Database = savedConfiguration.Database;
                configuration.Ftp = savedConfiguration.Ftp;
                configuration.Timestamp = savedConfiguration.Timestamp;
            }, false);


            await this.grpcManager.AddServicesAsync(new[]
                {Cida.CidaApiService.BindService(new GrpcManager.CidaApiService(this.moduleLoader))});
            await this.moduleLoader.Start();
        }

        private void ValidateFtp(ExternalServerConnection ftpConnection)
        {
            this.logger.Info("Validating ftp configuration");
            if (!this.ftpClient.ValidateConfiguration(ftpConnection))
            {
                throw new InvalidOperationException("FTP configuration is invalid.");
            }
            this.logger.Info("Done validating ftp configuration");
            this.logger.Info("Validating ftp connection");
            if (!this.ftpClient.TryConnect(ftpConnection, out var ex))
            {
                throw new InvalidOperationException("Couldn't connect to all FTP-Server. See inner exception for more details.", ex);
            }
            this.logger.Info("Done validating ftp connection");
        }

        private void ValidateDatabase(DatabaseConnection databaseConnection)
        {
            this.logger.Info("Validating database configuration");
            if (!this.databaseProvider.ValidateConfiguration(databaseConnection))
            {
                throw new InvalidOperationException("Database configuration is invalid.");
            }
            this.logger.Info("Done validating database configuration");
            this.logger.Info("Validating database connection");
            if (!this.databaseProvider.TryConnect(databaseConnection, out var ex))
            {
                throw new InvalidOperationException("Couldn't connect to database. See inner exception for more details.", ex);
            }
            this.logger.Info("Done validating database connection");
        }
    }

    public class ServerConfiguration
    {
        public string[] UnpackedModuleDirectories { get; set; } = Array.Empty<string>();
    }
}