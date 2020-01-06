using Cida.Server.Infrastructure.Database.BaseClasses;
using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
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
    public class DatabaseConnector : DatabaseConnectorBase
    {
        private CidaDbConnectionProvider databaseConnectionProvider;

        public DatabaseConnector(CidaContextBase context, CidaDbConnectionProvider databaseConnectionProvider, GlobalConfigurationManager globalConfigurationManager) : base(context, globalConfigurationManager)
        {
            this.databaseConnectionProvider = databaseConnectionProvider;
        }        

        private async Task CreateDatabaseInstanceAsync(Guid moduleId, string password)
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

        protected override async Task<DatabaseInformation> CreateDatabaseAsync(Guid moduleId, string password, ModuleInformation moduleInformation)
        {
            await this.CreateDatabaseInstanceAsync(moduleId, password);

            return await base.CreateDatabaseAsync(moduleId, password, moduleInformation);
        }
    }
}
