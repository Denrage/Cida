using Cida.Api;
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Models;
using static Module.AnimeSchedule.Cida.Services.Source.NiblAnimeInfoHandler;

namespace Module.AnimeSchedule.Cida.Services;

public class AnimeScheduleDbContext : DbContext
{
    private readonly string connectionString;
    private readonly IDatabaseProvider databaseProvider;

    public DbSet<Schedule> Schedules {  get; set; }

    public DbSet<AnimeInfo> AnimeInfos {  get; set; }

    public DbSet<Episode> Episodes {  get; set; }

    public DbSet<CrunchyrollEpisode> CrunchyrollEpisodes {  get; set; }

    public DbSet<Package> Packages {  get; set; }

    public DbSet<AnimeFolder> AnimeFolders {  get; set; }

    public DbSet<AnimeFilter> AnimeFilters {  get; set; }

    public DbSet<DatabaseSettings> Settings {  get; set; }

    public DbSet<DiscordWebhook> DiscordWebhooks {  get; set; }

    public AnimeScheduleDbContext(string connectionString, IDatabaseProvider databaseProvider)
    {
        this.connectionString = connectionString;
        this.databaseProvider = databaseProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>().HasKey(e => e.Id);
        modelBuilder.Entity<Schedule>().HasMany(x => x.Animes).WithMany(x => x.Schedules);
        modelBuilder.Entity<Schedule>().HasMany(x => x.DiscordWebhooks).WithMany(x => x.Schedules);
        modelBuilder.Entity<Schedule>().HasMany(x => x.Episodes).WithMany(x => x.Schedules);

        modelBuilder.Entity<AnimeInfo>().HasKey(e => e.Id);
        modelBuilder.Entity<AnimeInfo>().HasMany(x => x.Episodes).WithOne(x => x.Anime);
        modelBuilder.Entity<AnimeInfo>().HasOne(x => x.AnimeFilter).WithOne(x => x.Anime).HasForeignKey<AnimeFilter>(x => x.AnimeId);
        modelBuilder.Entity<AnimeInfo>().HasOne(x => x.AnimeFolder).WithOne(x => x.Anime).HasForeignKey<AnimeFolder>(x => x.AnimeId);

        modelBuilder.Entity<Episode>().HasKey(e => e.Name);
        modelBuilder.Entity<Episode>().HasOne(x => x.CrunchyrollEpisode).WithOne(x => x.Episode).HasForeignKey<CrunchyrollEpisode>(x => x.EpisodeName);
        modelBuilder.Entity<Episode>().HasOne(x => x.Package).WithOne(x => x.Episode).HasForeignKey<Package>(x => x.EpisodeName);

        modelBuilder.Entity<AnimeFilter>().HasKey(e => e.AnimeId);
        
        modelBuilder.Entity<AnimeFolder>().HasKey(e => e.AnimeId);

        modelBuilder.Entity<CrunchyrollEpisode>().HasKey(e => e.EpisodeName);

        modelBuilder.Entity<Package>().HasKey(e => e.EpisodeName);

        modelBuilder.Entity<DiscordWebhook>().HasKey(e => e.WebhookId);

        modelBuilder.Entity<DatabaseSettings>().HasKey(e => e.Key);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        this.databaseProvider.OnConfiguring(optionsBuilder, this.connectionString); 
    }
}
