using Cida.Server.Infrastructure.Database.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Extensions
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
