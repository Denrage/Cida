using Cida.Server.Infrastructure.Database.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Server.Infrastructure.Database
{
    public class DatabaseConnector
    {
        private Task<bool> databaseCreationTask;
        private CidaContext context;
        private CidaDbConnectionProvider databaseConnectionProvider;

        public DatabaseConnector(CidaContext context, CidaDbConnectionProvider databaseConnectionProvider)
        {
            this.context = context;
            this.databaseConnectionProvider = databaseConnectionProvider;

            this.context.Database.EnsureCreated();
        }

        public string GetDatabaseConnectionString(Guid moduleId, string password)
        {
            var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection();
            var moduleInformation = this.context.Modules.Find(moduleId);

            if(moduleInformation == null)
            {
                throw new Exception("Module not found");
            }

            var databases = this.context.Databases.Where(db => db.Module.ModuleId == moduleId);
            DatabaseInformation databaseInformation;

            if(databases.Count() == 0)
            {
                this.CreateDatabase(moduleId, password);
                databaseInformation = new DatabaseInformation()
                {
                    Module = moduleInformation,
                    Username = $"ModuleUser_{moduleId}",
                    Password = password,
                    DatabaseName = $"ModuleDb_{moduleId}",
                };

                this.context.Databases.Add(databaseInformation);
                this.context.SaveChanges();
            }

            else if(databases.Count() > 1)
            {
                throw new Exception($"Multiple databases ({databases.Count()}) found.");
            }

            else
            {
                databaseInformation = databases.First();
                if(databaseInformation.Password != password)
                {
                    throw new Exception("Wrong password");
                }
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = dbConnection.DataSource,
                InitialCatalog = databaseInformation.DatabaseName,
                UserID = databaseInformation.Username,
                Password = databaseInformation.Password
            };

            return connectionStringBuilder.ToString();
        }

        public async Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password)
        {
            var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection();
            var moduleInformation = await this.context.Modules.FindAsync(moduleId);

            if (moduleInformation == null)
            {
                throw new Exception("Module not found");
            }

            var databases = this.context.Databases.Where(db => db.Module.ModuleId == moduleId);
            DatabaseInformation databaseInformation;

            if (databases.Count() == 0)
            {
                await this.CreateDatabaseAsync(moduleId, password);
                databaseInformation = new DatabaseInformation()
                {
                    Module = moduleInformation,
                    Username = $"ModuleUser_{moduleId}",
                    Password = password,
                    DatabaseName = $"ModuleDb_{moduleId}",
                };

                await this.context.Databases.AddAsync(databaseInformation);
                await this.context.SaveChangesAsync();
            }

            else if (databases.Count() > 0)
            {
                throw new Exception($"Multiple databases ({databases.Count()}) found.");
            }

            else
            {
                databaseInformation = databases.First();
                if (databaseInformation.Password != password)
                {
                    throw new Exception("Wrong password");
                }
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = dbConnection.DataSource,
                InitialCatalog = databaseInformation.DatabaseName,
                UserID = databaseInformation.Username,
                Password = databaseInformation.Password
            };

            return connectionStringBuilder.ToString();
        }

        private void CreateDatabase(Guid moduleId, string password)
        {
            var dbName = $"ModuleDb_{moduleId}";
            var username = $"ModuleUser_{moduleId}";

            using (var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection())
            {
                dbConnection.Open();
                var transaction = dbConnection.BeginTransaction();

                var createDbSql = $@"   CREATE DATABASE [{dbName}];
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

        private async Task CreateDatabaseAsync(Guid moduleId, string password)
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
