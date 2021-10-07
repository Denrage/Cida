using Cida.Server.Infrastructure.Database.ProviderLoader;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database;

public class CidaContext : CidaContextBase
{
    private CidaDbConnectionProvider databaseConnectionProvider;
    private readonly IDatabaseProvidersProvider databaseProvider;

    public CidaContext(CidaDbConnectionProvider databaseConnectionProvider, IDatabaseProvidersProvider databaseProvider)
    {
        this.databaseConnectionProvider = databaseConnectionProvider;
        this.databaseProvider = databaseProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = databaseConnectionProvider.GetDatabaseConnection();

        databaseProvider.SelectedProvider?.OnConfiguring(optionsBuilder, connection);
    }
}
