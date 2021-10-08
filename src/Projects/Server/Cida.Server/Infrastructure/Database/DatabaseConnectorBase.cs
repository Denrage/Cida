using System.Data.SqlClient;
using Cida.Server.Infrastructure.Database.ProviderLoader;
using Cida.Api;
using Cida.Server.Infrastructure.Database.Models;

namespace Cida.Server.Infrastructure.Database;

public abstract class DatabaseConnectorBase : IDatabaseConnector
{
    public event Action? OnConnectionChanged;

    protected Func<CidaContextBase> GetContext { get; }
    protected GlobalConfigurationService GlobalConfigurationService;
    protected IDatabaseProvidersProvider DatabaseProvidersProvider { get; }
    protected CidaDbConnectionProvider DbConnectionProvider { get; }

    public DatabaseConnectorBase(
        Func<CidaContextBase> getContext, 
        GlobalConfigurationService globalConfigurationService, 
        IDatabaseProvidersProvider databaseProvidersProvider,
        CidaDbConnectionProvider dbConnectionProvider)
    {
        this.GetContext = getContext;
        this.GlobalConfigurationService = globalConfigurationService;
        this.DatabaseProvidersProvider = databaseProvidersProvider;
        this.DbConnectionProvider = dbConnectionProvider;
        this.DbConnectionProvider.ConnectionStringUpdated += OnConnectionChanged;
    }

    public async Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password)
    {
        using var context = this.GetContext();
        if (context.Modules is null || context.Databases is null)
        {
            throw new InvalidOperationException("Database is not initialized!");
        }

        var moduleInformation = await context.Modules.FindAsync(moduleId);

        if (moduleInformation == null)
        {
            throw new Exception("Module not found");
        }

        var databases = context.Databases.Where(db => db.Module.ModuleId == moduleId);
        DatabaseInformation databaseInformation;

        if (databases.Count() == 0)
        {
            databaseInformation = await CreateDatabaseAsync(moduleId, password, moduleInformation);
        }
        else if (databases.Count() > 1)
        {
            throw new Exception($"Multiple databases ({databases.Count()}) found.");
        }
        else
        {
            databaseInformation = databases.First();
            if (databaseInformation.Password != password)
            {
                throw new Exception("Wrong password");
            }
        }

        return BuildConnectionString(databaseInformation);
    }

    protected virtual async Task<DatabaseInformation> CreateDatabaseAsync(Guid moduleId, string password, ModuleInformation moduleInformation)
    {
        using var context = this.GetContext();
        if (context.Databases is null)
        {
            throw new InvalidOperationException("Database is not initialized!");
        }

        var databaseInformation = new DatabaseInformation()
        {
            DatabaseName = $"ModuleDb_{moduleId:N}",
            Username = $"ModuleUser_{moduleId:N}",
            Password = password,
            ModuleId = moduleId,
        };

        await context.Databases.AddAsync(databaseInformation);
        await context.SaveChangesAsync();
        return databaseInformation;
    }

    protected string BuildConnectionString(DatabaseInformation databaseInformation)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = GlobalConfigurationService.ConfigurationManager.Database.Connection.Host,
            InitialCatalog = databaseInformation.DatabaseName,
            UserID = databaseInformation.Username,
            Password = databaseInformation.Password
        };

        return connectionStringBuilder.ToString();
    }

    public IDatabaseProvider GetDatabaseProvider()
        => DatabaseProvidersProvider.SelectedProvider;
}
