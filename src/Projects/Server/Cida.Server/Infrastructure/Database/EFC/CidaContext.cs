using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Cida.Server.Infrastructure.Database.ProviderLoader;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database.EFC;

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
        var connection = this.databaseConnectionProvider.GetDatabaseConnection();

        this.databaseProvider.SelectedProvider?.OnConfiguring(optionsBuilder, connection);
    }
}
