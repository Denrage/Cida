using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Models;
using static Module.AnimeSchedule.Cida.Services.Source.NiblAnimeInfoHandler;

namespace Module.AnimeSchedule.Cida.Services;

public class AnimeScheduleDbContext : DbContext
{
    private readonly string connectionString;

    public DbSet<Schedule> Schedules {  get; set; }

    public DbSet<AnimeInfo> AnimeInfos {  get; set; }

    public DbSet<Episode> Episodes {  get; set; }

    public DbSet<CrunchyrollEpisode> CrunchyrollEpisodes {  get; set; }

    public DbSet<Package> Packages {  get; set; }

    public DbSet<AnimeFolder> AnimeFolders {  get; set; }

    public DbSet<AnimeFilter> AnimeFilters {  get; set; }

    public DbSet<DatabaseSettings> Settings {  get; set; }

    public DbSet<DiscordWebhook> DiscordWebhooks {  get; set; }

    public AnimeScheduleDbContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>().HasKey(e => e.Id);
        modelBuilder.Entity<Schedule>().HasMany(x => x.Animes).WithOne(x => x.Schedule);
        modelBuilder.Entity<Schedule>().HasMany(x => x.DiscordWebhooks).WithMany(x => x.Schedules);

        modelBuilder.Entity<AnimeInfo>().HasKey(e => e.Id);
        modelBuilder.Entity<AnimeInfo>().HasMany(x => x.Episodes).WithOne(x => x.Anime);
        modelBuilder.Entity<AnimeInfo>().HasOne(x => x.AnimeFilter).WithOne(x => x.Anime);
        modelBuilder.Entity<AnimeInfo>().HasOne(x => x.AnimeFolder).WithOne(x => x.Anime);

        modelBuilder.Entity<Episode>().HasKey(e => e.Name);
        modelBuilder.Entity<Episode>().HasOne(x => x.CrunchyrollEpisode).WithOne(x => x.Episode);
        modelBuilder.Entity<Episode>().HasOne(x => x.Package).WithOne(x => x.Episode);

        modelBuilder.Entity<DatabaseSettings>().HasKey(e => e.Key);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }
}
