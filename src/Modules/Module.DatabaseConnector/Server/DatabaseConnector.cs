using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Module.DatabaseConnector.EFC;
using Module.DatabaseConnector.Models.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            this.context.Database.EnsureCreated();
        }

        public string GetDatabaseConnectionString(string moduleId, string password)
        {
            var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection();

            var modules = this.context.Modules;
            var moduleInformation = modules.Find(moduleId);

            if (moduleInformation == null)
            {
                this.CreateDatabase(moduleId, password);
                moduleInformation = new ModuleInformation()
                {
                    ModuleId = moduleId,
                    ModuleName = "temp",
                    Username = $"ModuleUser_{moduleId}",
                    Password = password,
                    DatabaseName = $"ModuleDb_{moduleId}"
                };

                modules.Add(moduleInformation);
                this.context.SaveChanges();
            }

            else if (moduleInformation.DatabaseName == null)
            {
                this.CreateDatabase(moduleId, password);
                moduleInformation.Username = $"ModuleUser_{moduleId}";
                moduleInformation.Password = password;
                moduleInformation.DatabaseName = $"ModuleDb_{moduleId}";
                this.context.SaveChanges();

            if (moduleInformation.Password != password)
            {
                throw new Exception("Wrong login");
            }
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

        public async Task<string> GetDatabaseConnectionStringAsync(string moduleId, string password)
        {
            var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection();

            var modules = this.context.Modules;
            var moduleInformation = await modules.FindAsync(moduleId);

            if (moduleInformation == null)
            {
                await this.CreateDatabaseAsync(moduleId, password);
                moduleInformation = new ModuleInformation()
                {
                    ModuleId = moduleId,
                    ModuleName = "temp",
                    Username = $"ModuleUser_{moduleId}",
                    Password = password,
                    DatabaseName = $"ModuleDb_{moduleId}"
                };

                await modules.AddAsync(moduleInformation);
                await this.context.SaveChangesAsync();
            }

            else if (moduleInformation.DatabaseName == null)
            {
                await this.CreateDatabaseAsync(moduleId, password);
                moduleInformation.Username = $"ModuleUser_{moduleId}";
                moduleInformation.Password = password;
                moduleInformation.DatabaseName = $"ModuleDb_{moduleId}";
                await this.context.SaveChangesAsync();

                if (moduleInformation.Password != password)
                {
                    throw new Exception("Wrong login");
                }
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
            var dbName = $"ModuleDb_{moduleId}";
            var username = $"ModuleUser_{moduleId}";

            using (var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection())
            {
                dbConnection.Open();
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
                dbConnection.Close();
            }
        }

        private async Task CreateDatabaseAsync(string moduleId, string password)
        {
            var dbName = $"ModuleDb_{moduleId}";
            var username = $"ModuleUser_{moduleId}";

            using (var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection())
            {
                await dbConnection.OpenAsync();
                var transaction = await dbConnection.BeginTransactionAsync();

                var createDbSql = $@"   CREATE DATABASE {dbName};
                                ";
                var createUserSql = $@" USE [master];
                                    CREATE LOGIN [{username}] WITH PASSWORD=N'{password}', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
                                    USE [{dbName}];
                                    CREATE USER [{username}] FOR LOGIN [{username}];
                                    ALTER ROLE [db_owner] ADD MEMBER [{username}];
                                ";
                await this.context.Database.ExecuteSqlRawAsync(createDbSql);
                await this.context.Database.ExecuteSqlRawAsync(createUserSql);
                await transaction.CommitAsync();
                await dbConnection.CloseAsync();
            }
        }
    }
}
