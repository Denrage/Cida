using Microsoft.EntityFrameworkCore;
using Module.DatabaseConnector.Models;
using Module.DatabaseConnector.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.DatabaseConnector.EFC
{
    public class CidaContext : DbContext
    {
        CidaDbConnectionProvider databaseConnectionProvider;

        public CidaContext(CidaDbConnectionProvider databaseConnectionProvider)
        {
            this.databaseConnectionProvider = databaseConnectionProvider;
        }

        public DbSet<ModuleInformation> Modules { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = this.databaseConnectionProvider.GetDatabaseConnection();

            optionsBuilder.UseSqlServer(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ModuleInformation>(moduleInformation =>
            {
                moduleInformation.HasKey(x => x.ModuleId);
                moduleInformation.Property(x => x.ModuleId).IsRequired().HasComment("Unique ID of a Cida module.");
                moduleInformation.Property(x => x.ModuleName).IsRequired().HasComment("Name of the module with the given ID.");
                moduleInformation.Property(x => x.DatabaseName).HasComment("Database name for login.");
                moduleInformation.Property(x => x.Username).HasComment("Username used for login to the given database.");
                moduleInformation.Property(x => x.Password).HasComment("Password used for login to the given database.");
            });
        }
    }
}
