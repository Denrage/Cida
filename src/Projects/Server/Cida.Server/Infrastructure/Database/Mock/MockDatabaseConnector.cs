using Cida.Server.Infrastructure.Database.ProviderLoader;

namespace Cida.Server.Infrastructure.Database.Mock;

public class MockDatabaseConnector : DatabaseConnectorBase
{
    public MockDatabaseConnector(GlobalConfigurationService globalConfigurationService, CidaContextBase context, IDatabaseProvidersProvider databaseProvidersProvider)
        : base(context, globalConfigurationService, databaseProvidersProvider)
    {
    }
}
