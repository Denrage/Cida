using Microsoft.EntityFrameworkCore;
using Module.HorribleSubs.Cida.Models.Database;

namespace Module.HorribleSubs.Cida.Services
{
    public class HorribleSubsDbContext : DbContext
    {
        private readonly string connectionString;
        public DbSet<Download> Downloads { get; set; }

        public HorribleSubsDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Download>().HasKey(x => x.Name);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}