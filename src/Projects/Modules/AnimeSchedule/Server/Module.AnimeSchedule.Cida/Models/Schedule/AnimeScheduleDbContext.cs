using Microsoft.EntityFrameworkCore;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class AnimeScheduleDbContext : DbContext
    {
        private readonly string connectionString;

        public DbSet<Models.Database.Settings> Settings { get; set; }

        public DbSet<Models.Database.Schedule> Schedules { get; set; }

        public DbSet<Models.Database.AnimeContext> AnimeContexts { get; set; }

        public DbSet<Models.Database.Episode> Episodes { get; set; }

        public DbSet<Models.Database.PackageNumber> PackageNumbers { get; set; }

        public AnimeScheduleDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Database.Settings>().HasKey(x => x.Name);

            modelBuilder.Entity<Models.Database.Schedule>().HasKey(x => x.Id);
            modelBuilder.Entity<Models.Database.Schedule>().HasMany(x => x.AnimeContexts).WithOne(x => x.Schedule);

            modelBuilder.Entity<Models.Database.AnimeContext>().HasKey(x => x.MyAnimeListId);
            modelBuilder.Entity<Models.Database.AnimeContext>().HasMany(x => x.Episodes).WithOne(x => x.AnimeContext);

            modelBuilder.Entity<Models.Database.Episode>().HasKey(x => x.Name);
            modelBuilder.Entity<Models.Database.Episode>().HasOne(x => x.PackageNumber).WithOne(x => x.Episode).HasForeignKey<Models.Database.PackageNumber>(x => x.Name);

            modelBuilder.Entity<Models.Database.PackageNumber>().HasKey(x => new { x.Number, x.Name });
            modelBuilder.Entity<Models.Database.PackageNumber>().HasOne(x => x.Episode).WithOne(x => x.PackageNumber);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
