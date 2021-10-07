using Cida.Api.Database;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Server.Infrastructure.Database.ProviderLoader
{
    public class DatabaseProviderLoader : IDatabaseProvidersProvider
    {
        public const string DatabaseProviderFolderName = "DatabaseProviders";
        private const string DatabaseProviderFileExtension = "cidadbp"; // (Cida) (D)ata(b)ase (P)rovider
        private readonly string providerFolder;
        private readonly ILogger logger;
        private readonly GlobalConfigurationService globalConfigurationService;
        private readonly List<DatabaseProvider> providers = new();

        public IReadOnlyList<IDatabaseProvider> Providers => this.providers.Select(x => x.Provider).ToList().AsReadOnly();

        public IDatabaseProvider? SelectedProvider { get; private set; }

        public DatabaseProviderLoader(string providerFolder, ILogger logger, GlobalConfigurationService globalConfigurationService)
        {
            this.providerFolder = providerFolder;
            this.logger = logger;
            this.globalConfigurationService = globalConfigurationService;
            this.globalConfigurationService.ConfigurationChanged += () => this.SelectProvider();
        }

        public void Load()
        {
            this.logger.Info("Loading database providers");
            foreach (var item in this.providers)
            {
                item.Dispose();
            }
            this.providers.Clear();

            foreach (var file in Directory.GetFiles(this.providerFolder, $"*.{DatabaseProviderFileExtension}"))
            {
                DatabaseProvider? provider = null;
                this.logger.Info($"Loading database provider '{file}'");
                try
                {
                    provider = DatabaseProvider.Extract(file);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, $"Error parsing database provider '{file}'");
                }

                if (provider != null)
                {
                    this.providers.Add(provider);
                }
                else
                {
                    this.logger.Warn($"Database Provider '{file}' couldn't be extracted!");
                }
                this.logger.Info("Finished loading database provider");
            }

            this.logger.Info("Finished loading database providers");
        }

        private void SelectProvider()
        {
            this.logger.Info($"Selecting provider for '{globalConfigurationService.ConfigurationManager.Database.DatabaseType}'");
            if (providers.Where(x => x.Provider.DatabaseType == globalConfigurationService.ConfigurationManager.Database.DatabaseType).Any())
            {
                this.SelectedProvider = providers
                    .First(x => x.Provider.DatabaseType == globalConfigurationService.ConfigurationManager.Database.DatabaseType)
                    .Provider;
            }
            else
            {
                throw new InvalidOperationException("Selected provider does not exist!");
            }
        }
    }
}
