using Microsoft.Data.SqlClient;
using Module.DatabaseConnector.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.DatabaseConnector.Extensions
{
    public static class ConnectionSettingsExtensions
    {
        public static string ToConnectionString(this ConnectionSettings connectionSettings)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = connectionSettings.ServerName,
                InitialCatalog = connectionSettings.DatabaseName,
                UserID = connectionSettings.Username,
                Password = connectionSettings.Password
            };

            return connectionStringBuilder.ToString();
        }
    }
}
