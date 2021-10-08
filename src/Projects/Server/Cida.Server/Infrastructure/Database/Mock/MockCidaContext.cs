using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database.Mock;

public class MockCidaContext : CidaContextBase
{
    private GlobalConfigurationManager globalConfigurationManager;

    public MockCidaContext(GlobalConfigurationManager globalConfigurationManager)
    {
        this.globalConfigurationManager = globalConfigurationManager;

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var databaseName = globalConfigurationManager.Database.DatabaseName;

        optionsBuilder.UseInMemoryDatabase(databaseName);
    }
}
