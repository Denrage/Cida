using Microsoft.EntityFrameworkCore;

namespace Module.AnimeSchedule.Cida.Services
{
    public class AnimeScheduleDbContext : DbContext
    {
        private readonly string connectionString;

        public AnimeScheduleDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}