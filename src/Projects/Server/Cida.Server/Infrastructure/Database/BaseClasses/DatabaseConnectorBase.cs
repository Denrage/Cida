using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Cida.Api;

namespace Cida.Server.Infrastructure.Database.BaseClasses
{
    public abstract class DatabaseConnectorBase : IDatabaseConnector
    {
        protected CidaContextBase Context;
        protected GlobalConfigurationService GlobalConfigurationService;

        public DatabaseConnectorBase(CidaContextBase context, GlobalConfigurationService globalConfigurationService)
        {
            this.Context = context;
            this.GlobalConfigurationService = globalConfigurationService;
        }

        public async Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password)
        {
            if (this.Context.Modules is null || this.Context.Databases is null)
            {
                throw new InvalidOperationException("Database is not initialized!");
            }

            var moduleInformation = await this.Context.Modules.FindAsync(moduleId);

            if (moduleInformation == null)
            {
                throw new Exception("Module not found");
            }

            var databases = this.Context.Databases.Where(db => db.Module.ModuleId == moduleId);
            DatabaseInformation databaseInformation;

            if (databases.Count() == 0)
            {
                databaseInformation = await this.CreateDatabaseAsync(moduleId, password, moduleInformation);
            }
            else if (databases.Count() > 1)
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

            return this.BuildConnectionString(databaseInformation);
        }

        protected virtual async Task<DatabaseInformation> CreateDatabaseAsync(Guid moduleId, string password, ModuleInformation moduleInformation)
        {
            if (this.Context.Databases is null)
            {
                throw new InvalidOperationException("Database is not initialized!");
            }

            var databaseInformation = new DatabaseInformation()
            {
                DatabaseName = $"ModuleDb_{moduleId:N}",
                Username = $"ModuleUser_{moduleId:N}",
                Password = password,
                ModuleId = moduleId,
                Module = moduleInformation,
            };

            await this.Context.Databases.AddAsync(databaseInformation);
            await this.Context.SaveChangesAsync();
            return databaseInformation;
        }

        protected string BuildConnectionString(DatabaseInformation databaseInformation)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = this.GlobalConfigurationService.ConfigurationManager.Database.Connection.Host,
                InitialCatalog = databaseInformation.DatabaseName,
                UserID = databaseInformation.Username,
                Password = databaseInformation.Password
            };

            return connectionStringBuilder.ToString();
        }
    }
}
