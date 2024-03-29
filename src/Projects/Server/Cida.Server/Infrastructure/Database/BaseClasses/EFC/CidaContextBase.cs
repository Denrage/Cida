﻿using Cida.Server.Infrastructure.Database.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database.BaseClasses.EFC;

public abstract class CidaContextBase : DbContext
{
    public DbSet<ModuleInformation> Modules { get; set; } = null!;

    public DbSet<FtpInformation> FtpPaths { get; set; } = null!;

    public DbSet<DatabaseInformation> Databases { get; set; } = null!;

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
