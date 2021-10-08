using Cida.Server.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Cida.Server.Infrastructure.Database;

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

            moduleInformation.Property(x => x.ModuleId).IsRequired();
            moduleInformation.Property(x => x.ModuleName).IsRequired();
        });

        modelBuilder.Entity<FtpInformation>(ftpInformation =>
        {
            ftpInformation.HasOne(x => x.Module).WithOne(x => x.FtpInfomation).HasForeignKey<FtpInformation>(x => x.ModuleId).IsRequired();
            ftpInformation.HasKey(x => x.ModuleId);

            ftpInformation.Property(x => x.ModuleId).IsRequired();
            ftpInformation.Property(x => x.FtpPath).IsRequired();
        });

        modelBuilder.Entity<DatabaseInformation>(databaseInformation =>
        {
            databaseInformation.HasOne(x => x.Module).WithOne(x => x.DatabaseInformation).HasForeignKey<DatabaseInformation>(x => x.ModuleId).IsRequired();
            databaseInformation.HasKey(x => x.ModuleId);

            databaseInformation.Property(x => x.DatabaseName);
            databaseInformation.Property(x => x.Username);
            databaseInformation.Property(x => x.Password);
        });
    }
}
