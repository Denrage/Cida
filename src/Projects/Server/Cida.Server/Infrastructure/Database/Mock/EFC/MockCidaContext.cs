using Cida.Server.Infrastructure.Database.BaseClasses.EFC;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database.Mock.EFC
{
    public class MockCidaContext : CidaContextBase
    {
        private GlobalConfigurationManager globalConfigurationManager;

        public MockCidaContext(GlobalConfigurationManager globalConfigurationManager)
        {
            this.globalConfigurationManager = globalConfigurationManager;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databaseName = this.globalConfigurationManager.Database.DatabaseName;

            optionsBuilder.UseInMemoryDatabase(databaseName);
        }
    }
}
