using Microsoft.EntityFrameworkCore;
using Module.IrcAnime.Cida.Models.Database;

namespace Module.IrcAnime.Cida.Services
{
    public class IrcAnimeDbContext : DbContext
    {
        private readonly string connectionString;
        public DbSet<Download> Downloads { get; set; }

        public IrcAnimeDbContext(string connectionString)
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