﻿using Cida.Server.Infrastructure.Database.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;

namespace Cida.Server.Infrastructure.Database
{
    public class CidaDbConnectionProvider
    {
        private readonly GlobalConfigurationService configurationService;

        public event Action? ConnectionStringUpdated;

        public string? ConnectionString { get; private set; }

        public CidaDbConnectionProvider(GlobalConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.configurationService.ConfigurationChanged += this.UpdateConnectionString;
        }


        public DbConnection GetDatabaseConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }

        private void UpdateConnectionString()
        {
            if (this.configurationService.ConfigurationManager?.Database?.Connection?.Host != null)
            {
                this.ConnectionString = this.configurationService.ConfigurationManager.Database.ToConnectionString();
                this.ConnectionStringUpdated?.Invoke();
            }
        }

        public bool ValidateConfiguration(DatabaseConnection connectionSettings)
        {
            if (string.IsNullOrEmpty(connectionSettings.DatabaseName))
            {
                return false;
            }

            if (connectionSettings.Connection == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(connectionSettings.Connection.Host) ||
                string.IsNullOrEmpty(connectionSettings.Connection.Username))
            {
                return false;
            }

            return true;
        }
    }
}
