using Cida.Server.Infrastructure.Database.BaseClasses;
using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Cida.Server.Infrastructure.Database.ProviderLoader;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database
{
    public class DatabaseConnector : DatabaseConnectorBase
    {
        private CidaDbConnectionProvider databaseConnectionProvider;
        private readonly IDatabaseProvidersProvider databaseProvider;

        public DatabaseConnector(
            CidaContextBase context, 
            CidaDbConnectionProvider databaseConnectionProvider, 
            GlobalConfigurationService globalConfigurationService,
            IDatabaseProvidersProvider databaseProvider) 
            : base(context, globalConfigurationService)
        {
            this.databaseConnectionProvider = databaseConnectionProvider;
            this.databaseProvider = databaseProvider;
        }        

        private async Task CreateDatabaseInstanceAsync(Guid moduleId, string password)
        {
            if (this.databaseProvider.SelectedProvider is null)
            {
                throw new InvalidOperationException("There is no selected database provider available");
            }

            var dbName = $"ModuleDb_{moduleId:N}";
            var username = $"ModuleUser_{moduleId:N}";

            using (var dbConnection = this.databaseConnectionProvider.GetDatabaseConnection())
            {
                await dbConnection.OpenAsync();
                var transaction = await dbConnection.BeginTransactionAsync();

                var createDbSql = this.databaseProvider.SelectedProvider.CreateDbSql(dbName);
                var createUserSql = this.databaseProvider.SelectedProvider.CreateUserSql(username, password, dbName); 
                await this.Context.Database.ExecuteSqlRawAsync(createDbSql);
                await this.Context.Database.ExecuteSqlRawAsync(createUserSql);
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
