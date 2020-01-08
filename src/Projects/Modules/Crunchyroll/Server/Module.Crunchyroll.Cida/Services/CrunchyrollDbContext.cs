using Microsoft.EntityFrameworkCore;
using Module.Crunchyroll.Libs.Models.Database;

namespace Module.Crunchyroll.Cida.Services
{
    public class CrunchyrollDbContext : DbContext
    {
        private readonly string connectionString;
        public DbSet<Libs.Models.Database.Anime> Animes { get; set; }
        public DbSet<Libs.Models.Database.Image> Images { get; set; }
        public DbSet<Libs.Models.Database.Episode> Episodes { get; set; }
        public DbSet<Libs.Models.Database.Collection> Collections { get; set; }

        public CrunchyrollDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Libs.Models.Database.Image>().HasKey(x => x.Id);
            modelBuilder.Entity<Anime>(anime =>
            {
                anime.HasKey(x => x.Id);
                anime.HasOne(x => x.Portrait).WithOne().HasForeignKey<Anime>(x => x.PortraitId);
                anime.HasOne(x => x.Landscape).WithOne().HasForeignKey<Anime>(x => x.LandscapeId);
            });
            modelBuilder.Entity<Libs.Models.Database.Episode>(episode =>
            {
                episode.HasKey(x => x.Id);
                episode.HasOne(x => x.Collection);
                episode.HasOne(x => x.Image).WithOne().HasForeignKey<Episode>(x => x.ImageId);
            });

            modelBuilder.Entity<Libs.Models.Database.Collection>(collection =>
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