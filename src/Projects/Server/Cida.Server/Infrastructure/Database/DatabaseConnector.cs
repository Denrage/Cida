using Cida.Server.Infrastructure.Database.Models;
using Cida.Server.Infrastructure.Database.ProviderLoader;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database;

public class DatabaseConnector : DatabaseConnectorBase
{
    public DatabaseConnector(
        Func<CidaContextBase> getContext,
        CidaDbConnectionProvider databaseConnectionProvider,
        GlobalConfigurationService globalConfigurationService,
        IDatabaseProvidersProvider databaseProvider)
        : base(getContext, globalConfigurationService, databaseProvider, databaseConnectionProvider)
    {
    }

    private async Task CreateDatabaseInstanceAsync(Guid moduleId, string password)
    {
        if (this.DatabaseProvidersProvider.SelectedProvider is null)
        {
            throw new InvalidOperationException("There is no selected database provider available");
        }

        var dbName = $"ModuleDb_{moduleId:N}";
        var username = $"ModuleUser_{moduleId:N}";

        using var dbConnection = this.DbConnectionProvider.GetDatabaseConnection();
        using var context = this.GetContext();
        await dbConnection.OpenAsync();
        var transaction = await dbConnection.BeginTransactionAsync();

        var createDbSql = this.DatabaseProvidersProvider.SelectedProvider.CreateDbSql(dbName);
        var createUserSql = this.DatabaseProvidersProvider.SelectedProvider.CreateUserSql(username, password, dbName);

        await context.Database.ExecuteSqlRawAsync(createDbSql);
        await context.Database.ExecuteSqlRawAsync(createUserSql);
        await transaction.CommitAsync();
        await dbConnection.CloseAsync();
    }

    protected override async Task<DatabaseInformation> CreateDatabaseAsync(Guid moduleId, string password, ModuleInformation moduleInformation)
    {
        await this.CreateDatabaseInstanceAsync(moduleId, password);

        return await base.CreateDatabaseAsync(moduleId, password, moduleInformation);
    }
}
