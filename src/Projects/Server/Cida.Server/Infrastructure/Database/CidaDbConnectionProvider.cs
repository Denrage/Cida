using Cida.Server.Infrastructure.Database.Extensions;
using Cida.Server.Infrastructure.Database.ProviderLoader;
using System.Data.Common;

namespace Cida.Server.Infrastructure.Database;

public class CidaDbConnectionProvider
{
    private readonly GlobalConfigurationService configurationService;
    private readonly IDatabaseProvidersProvider databaseProvidersProvider;

    public event Action? ConnectionStringUpdated;

    public string? ConnectionString { get; private set; }

    public CidaDbConnectionProvider(GlobalConfigurationService configurationService, IDatabaseProvidersProvider databaseProvidersProvider)
    {
        this.configurationService = configurationService;
        this.databaseProvidersProvider = databaseProvidersProvider;
        this.configurationService.ConfigurationChanged += this.UpdateConnectionString;
    }


    public DbConnection GetDatabaseConnection()
    {
        if (this.databaseProvidersProvider.SelectedProvider is null || this.ConnectionString is null)
        {
            throw new ArgumentNullException($"{nameof(this.databaseProvidersProvider.SelectedProvider)} or {nameof(this.ConnectionString)}");
        }

        return this.databaseProvidersProvider.SelectedProvider.GetDbConnection(this.ConnectionString);
    }

    private void UpdateConnectionString()
    {
        if (this.configurationService.ConfigurationManager?.Database?.Connection?.Host != null)
        {
            this.ConnectionString = this.configurationService.ConfigurationManager.Database.ToConnectionString();
            this.ConnectionStringUpdated?.Invoke();
        }
    }

    public bool ValidateConfiguration(DatabaseConnection connectionSettings)
    {
        if (string.IsNullOrEmpty(connectionSettings.DatabaseName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(connectionSettings.DatabaseType))
        {
            return false;
        }
        else if (!this.databaseProvidersProvider.Providers.Where(x => x.DatabaseType == connectionSettings.DatabaseType).Any())
        {
            return false;
        }

        if (connectionSettings.Connection == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(connectionSettings.Connection.Host) ||
            string.IsNullOrEmpty(connectionSettings.Connection.Username))
        {
            return false;
        }

        return true;
    }
}
