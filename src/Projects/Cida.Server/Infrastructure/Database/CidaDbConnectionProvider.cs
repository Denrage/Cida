using Cida.Server.Infrastructure.Database.Extensions;
using Cida.Server.Infrastructure.Database.Settings;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Cida.Server.Infrastructure.Database
{
    public class CidaDbConnectionProvider
    {
        private IDatabaseSettingsManager databaseSettingsManager;

        public CidaDbConnectionProvider(IDatabaseSettingsManager databaseSettingsManager)
        {
            this.databaseSettingsManager = databaseSettingsManager;
            this.databaseSettingsManager.settingsChanged += this.UpdateConnectionString;

            this.UpdateConnectionString();
        }

        public string ConnectionString { get; private set; }

        public DbConnection GetDatabaseConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }

        private void UpdateConnectionString()
        {
            var connectionSettings = this.databaseSettingsManager.LoadSettings();

            this.ConnectionString = connectionSettings.ToConnectionString();
        }
    }
}
