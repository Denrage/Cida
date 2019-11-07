using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Module.DatabaseConnector.EFC;
using Module.DatabaseConnector.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Module.DatabaseConnector
{
    public class DatabaseConnector
    {
        private CidaContext context;
        private CidaDbConnectionProvider databaseConnectionProvider;

        public DatabaseConnector(CidaContext context, CidaDbConnectionProvider databaseConnectionProvider)
        {
            this.context = context;
            this.databaseConnectionProvider = databaseConnectionProvider;

            this.context.Database.Migrate();
        }

        public string GetDatabaseConnectionString(string moduleId, string password)
        {
            var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection();

            var modules = this.context.Modules;
            var moduleInformation = modules.Find(moduleId);

            if(moduleInformation == null)
            {
                this.CreateDatabase(moduleId, password);
                moduleInformation = new ModuleInformation()
                {
                    ModuleId = moduleId,
                    ModuleName = "temp",
                    Username = $"ModuleUser{moduleId}",
                    Password = password,
                    DatabaseName = $"ModuleDb{moduleId}"
                };

                modules.Add(moduleInformation);
                this.context.SaveChanges();
            }

            if (moduleInformation.DatabaseName == null)
            {
                this.CreateDatabase(moduleId, password);
                moduleInformation.Username = $"ModuleUser{moduleId}";
                moduleInformation.Password = password;
                moduleInformation.DatabaseName = $"ModuleDb{moduleId}";
                this.context.SaveChanges();
            }

            if(moduleInformation.Password != password)
            {
                throw new Exception("Wrong login");
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = dbConnection.DataSource,
                InitialCatalog = moduleInformation.DatabaseName,
                UserID = moduleInformation.Username,
                Password = moduleInformation.Password
            };

            return connectionStringBuilder.ToString();
        }

        private void CreateDatabase(string moduleId, string password)
        {
            var dbName = $"ModuleDb{moduleId}";
            var username = $"ModuleUser{moduleId}";

            var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection();
            var transaction = dbConnection.BeginTransaction();

            var createDbSql = $@"   CREATE DATABASE {dbName};
                                ";
            var createUserSql = $@" USE [master];
                                    CREATE LOGIN [{username}] WITH PASSWORD=N'{password}', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
                                    USE [{dbName}];
                                    CREATE USER [{username}] FOR LOGIN [{username}];
                                    ALTER ROLE [db_owner] ADD MEMBER [{username}];
                                ";
            this.context.Database.ExecuteSqlRaw(createDbSql);
            this.context.Database.ExecuteSqlRaw(createUserSql);
            transaction.Commit();
        }
    }
}
