using Microsoft.EntityFrameworkCore;
using Module.Crunchyroll.Libs.Models.Database;

namespace Module.Crunchyroll.Cida.Services
{
    public class CrunchyrollDbContext : DbContext
    {
        private readonly string connectionString;
        public DbSet<Anime> Animes { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Collection> Collections { get; set; }

        public CrunchyrollDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>().HasKey(x => x.Id);
            modelBuilder.Entity<Anime>(anime =>
            {
                anime.HasKey(x => x.Id);
                anime.HasOne(x => x.Portrait).WithOne().HasForeignKey<Anime>(x => x.PortraitId);
                anime.HasOne(x => x.Landscape).WithOne().HasForeignKey<Anime>(x => x.LandscapeId);
            });
            modelBuilder.Entity<Episode>(episode =>
            {
                episode.HasKey(x => x.Id);
                episode.HasOne(x => x.Collection);
                episode.HasOne(x => x.Image).WithOne().HasForeignKey<Episode>(x => x.ImageId);
            });

            modelBuilder.Entity<Collection>(collection =>
            {
                collection.HasKey(x => x.Id);
                collection.HasOne(x => x.Anime);
                collection.HasMany(x => x.Episodes);
                collection.HasOne(x => x.Landscape).WithOne().HasForeignKey<Collection>(x => x.LandscapeId);
                collection.HasOne(x => x.Portrait).WithOne().HasForeignKey<Collection>(x => x.PortraitId);
            });
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.connectionString);
        }
    }
}