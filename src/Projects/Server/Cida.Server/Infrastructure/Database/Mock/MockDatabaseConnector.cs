using Cida.Server.Infrastructure.Database.BaseClasses;
using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Server.Infrastructure.Database.Mock
{
    public class MockDatabaseConnector : DatabaseConnectorBase
    {
        public MockDatabaseConnector(GlobalConfigurationManager globalConfigurationManager, CidaContextBase context) : base(context, globalConfigurationManager)
        {
        }
    }
}
