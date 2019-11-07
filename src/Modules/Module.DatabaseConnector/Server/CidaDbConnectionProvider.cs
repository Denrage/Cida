using Microsoft.Data.SqlClient;
using Module.DatabaseConnector.Extensions;
using Module.DatabaseConnector.Settings;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Module.DatabaseConnector
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
