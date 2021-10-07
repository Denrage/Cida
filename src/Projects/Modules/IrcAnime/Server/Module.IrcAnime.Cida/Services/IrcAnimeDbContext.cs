using Cida.Api;
using Microsoft.EntityFrameworkCore;
using Module.IrcAnime.Cida.Models.Database;

namespace Module.IrcAnime.Cida.Services
{
    public class IrcAnimeDbContext : DbContext
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public DbSet<Download> Downloads { get; set; }

        public IrcAnimeDbContext(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Download>().HasKey(x => x.Name);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            this.databaseProvider.OnConfiguring(optionsBuilder, this.connectionString);
        }
    }
}