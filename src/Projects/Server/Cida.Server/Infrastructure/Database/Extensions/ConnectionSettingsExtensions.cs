using Cida.Server.Infrastructure.Database.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Extensions
{
    public static class ConnectionSettingsExtensions
    {
        public static string ToConnectionString(this DatabaseConnection connectionSettings)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = connectionSettings.Connection.Host,
                InitialCatalog = connectionSettings.DatabaseName,
                UserID = connectionSettings.Connection.Username,
                Password = connectionSettings.Connection.Password
            };

            return connectionStringBuilder.ToString();
        }
        
        public static string ToConnectionString(this GlobalConfigurationManager.DatabaseConnectionManager connectionSettings)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = connectionSettings.Connection.Host,
                InitialCatalog = connectionSettings.DatabaseName,
                UserID = connectionSettings.Connection.Username,
                Password = connectionSettings.Connection.Password
            };

            return connectionStringBuilder.ToString();
        }
    }
}
