using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database.EFC
{
    public class CidaContext : CidaContextBase
    {
        private CidaDbConnectionProvider databaseConnectionProvider;

        public CidaContext(CidaDbConnectionProvider databaseConnectionProvider)
        {
            this.databaseConnectionProvider = databaseConnectionProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = this.databaseConnectionProvider.GetDatabaseConnection();

            optionsBuilder.UseSqlServer(connection);
        }
    }
}
