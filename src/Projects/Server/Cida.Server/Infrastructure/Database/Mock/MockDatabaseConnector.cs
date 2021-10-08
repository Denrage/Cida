using Cida.Server.Infrastructure.Database.ProviderLoader;

namespace Cida.Server.Infrastructure.Database.Mock;

public class MockDatabaseConnector : DatabaseConnectorBase
{
    public MockDatabaseConnector(GlobalConfigurationService globalConfigurationService, Func<CidaContextBase> getContext, IDatabaseProvidersProvider databaseProvidersProvider, CidaDbConnectionProvider cidaDbConnectionProvider)
        : base(getContext, globalConfigurationService, databaseProvidersProvider, cidaDbConnectionProvider)
    {
    }
}
