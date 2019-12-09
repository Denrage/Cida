using Cida.Server.Infrastructure.Database.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Cida.Server.Infrastructure.Database.Models;
using Cida.Server.Interfaces;

namespace Cida.Server.Infrastructure.Database
{
    public class CidaDbConnectionProvider
    {
        private readonly GlobalConfigurationService configurationService;

        public event Action ConnectionStringUpdated;
        
        public CidaDbConnectionProvider(GlobalConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.configurationService.ConfigurationChanged += this.UpdateConnectionString;
        }

        public string ConnectionString { get; private set; }

        public DbConnection GetDatabaseConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }

        private void UpdateConnectionString()
        {
            this.ConnectionString = this.configurationService.ConfigurationManager.Database.ToConnectionString();
            this.ConnectionStringUpdated?.Invoke();
        }
    }
}
