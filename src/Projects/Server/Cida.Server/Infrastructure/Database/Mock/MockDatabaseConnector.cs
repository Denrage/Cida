using Cida.Server.Infrastructure.Database.BaseClasses;
using Cida.Server.Infrastructure.Database.BaseClasses.EFC;

namespace Cida.Server.Infrastructure.Database.Mock
{
    public class MockDatabaseConnector : DatabaseConnectorBase
    {
        public MockDatabaseConnector(GlobalConfigurationService globalConfigurationService, CidaContextBase context) : base(context, globalConfigurationService)
        {
        }
    }
}
