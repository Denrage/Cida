using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Server.Infrastructure.Database.BaseClasses
{
    public abstract class DatabaseConnectorBase
    {
        protected GlobalConfigurationManager globalConfigurationManager;
        protected CidaContextBase context;

        public DatabaseConnectorBase(CidaContextBase context, GlobalConfigurationManager globalConfigurationManager)
        {
            this.context = context;
            this.globalConfigurationManager = globalConfigurationManager;
            this.context.Database.EnsureCreated();
        }

        public async Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password)
        {
            var moduleInformation = await this.context.Modules.FindAsync(moduleId);

            if (moduleInformation == null)
            {
                throw new Exception("Module not found");
            }

            var databases = this.context.Databases.Where(db => db.Module.ModuleId == moduleId);
            DatabaseInformation databaseInformation;

            if (databases.Count() == 0)
            {
                databaseInformation = await this.CreateDatabaseAsync(moduleId, password, moduleInformation);
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

            return this.BuildConnectionString(databaseInformation);
        }

        protected virtual async Task<DatabaseInformation> CreateDatabaseAsync(Guid moduleId, string password, ModuleInformation moduleInformation)
        {
            var databaseInformation = new DatabaseInformation()
            {
                Module = moduleInformation,
                Username = $"ModuleUser_{moduleId}",
                Password = password,
                DatabaseName = $"ModuleDb_{moduleId}",
            };
            await this.context.Databases.AddAsync(databaseInformation);
            await this.context.SaveChangesAsync();
            return databaseInformation;
        }

        protected string BuildConnectionString(DatabaseInformation databaseInformation)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = this.globalConfigurationManager.Database.Connection.Host,
                InitialCatalog = databaseInformation.DatabaseName,
                UserID = databaseInformation.Username,
                Password = databaseInformation.Password
            };

            return connectionStringBuilder.ToString();
        }
    }
}
