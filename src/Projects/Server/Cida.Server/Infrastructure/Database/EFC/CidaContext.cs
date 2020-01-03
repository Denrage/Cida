using Cida.Server.Infrastructure.Database;
using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.EFC
{
    public class CidaContext : DbContext
    {
        private CidaDbConnectionProvider databaseConnectionProvider;

        public CidaContext(CidaDbConnectionProvider databaseConnectionProvider)
        {
            this.databaseConnectionProvider = databaseConnectionProvider;
        }

        public DbSet<ModuleInformation> Modules { get; set; }

        public DbSet<FtpInformation> FtpPaths { get; set; }

        public DbSet<DatabaseInformation> Databases { get; set; }



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
                moduleInformation.HasOne(x => x.FtpInfomation).WithOne(x => x.Module);

                moduleInformation.Property(x => x.ModuleId).IsRequired().HasComment("Unique ID of a Cida module.");
                moduleInformation.Property(x => x.ModuleName).IsRequired().HasComment("Name of the module with the given ID.");
            });

            modelBuilder.Entity<FtpInformation>(ftpInformation =>
            {
                ftpInformation.HasOne(x => x.Module).WithOne(x => x.FtpInfomation).HasForeignKey<FtpInformation>(x => x.ModuleId).IsRequired();
                ftpInformation.HasKey(x => x.ModuleId);

                ftpInformation.Property(x => x.ModuleId).IsRequired().HasComment("Unique ID of a Cida modle.");
                ftpInformation.Property(x => x.FtpPath).IsRequired().HasComment("Path of the module on FTP-Server");
            });

            modelBuilder.Entity<DatabaseInformation>(databaseInformation =>
            {
                databaseInformation.HasOne(x => x.Module).WithOne(x => x.DatabaseInformation).HasForeignKey<DatabaseInformation>(x => x.ModuleId).IsRequired();
                databaseInformation.HasKey(x => x.ModuleId);

                databaseInformation.Property(x => x.DatabaseName).HasComment("Database name for login.");
                databaseInformation.Property(x => x.Username).HasComment("Username used for login to the given database.");
                databaseInformation.Property(x => x.Password).HasComment("Password used for login to the given database.");
            });
        }
    }
}
