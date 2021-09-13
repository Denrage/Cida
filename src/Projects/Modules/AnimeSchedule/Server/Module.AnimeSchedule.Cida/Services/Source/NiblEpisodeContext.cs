using Anilist4Net;
using Discord;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Services.Source;

public class NiblEpisodeContext : INotifyable, IDownloadable, IDatabaseSavable
{
    private readonly Client anilistClient;
    private readonly int scheduleId;

    public Package NiblPackage { get; }

    public bool AlreadyProcessed { get; set; } = false;

    public NiblEpisodeContext(Package package, Anilist4Net.Client anilistClient, int scheduleId)
    {
        this.NiblPackage = package;
        this.anilistClient = anilistClient;
        this.scheduleId = scheduleId;
    }

    public async Task<Embed> CreateEmbed()
    {
        var animeInfo = await this.anilistClient.GetMediaById((int)this.NiblPackage.Episode.AnimeId);
        var embedBuilder = new Discord.EmbedBuilder();
        embedBuilder
            .WithFooter(this.NiblPackage.BotName)
            .WithDescription("Episode: " + this.NiblPackage.Episode.EpisodeNumber.ToString() + (this.AlreadyProcessed ? " - Already available" : string.Empty))
            .WithTitle(animeInfo.RomajiTitle)
            .WithImageUrl(animeInfo.CoverImageLarge)
            .AddField("Original Name", this.NiblPackage.Episode.Name);

        return embedBuilder.Build();
    }

    public async Task<DownloadInformation> GetDownloadInformation(Func<AnimeScheduleDbContext> getContext, CancellationToken cancellationToken)
    {
        using var context = getContext();
        var dbAnimeFolder = await context.AnimeFolders.FindAsync(new object[] { this.NiblPackage.Episode.AnimeId }, cancellationToken); 
        var animeInfo = await this.anilistClient.GetMediaById((int)this.NiblPackage.Episode.AnimeId);
        return new DownloadInformation()
        {
            AnimeName = animeInfo.RomajiTitle,
            Bot = this.NiblPackage.BotName,
            DestinationFolderName = dbAnimeFolder.FolderName,
            FileName = this.NiblPackage.EpisodeName,
            PackageNumber = this.NiblPackage.PackageNumber,
        };
    }

    public async Task SaveToDatabase(AnimeScheduleDbContext context, CancellationToken cancellationToken)
    {
        var episode = await context.Episodes.FindAsync(new object[] { this.NiblPackage.Episode.Name }, cancellationToken);
        if (episode != null)
        {
            episode.Schedules.Add(await context.Schedules.FindAsync(new object[] { this.scheduleId }, cancellationToken));
            context.Episodes.Update(episode);
        }
        else
        {
            await context.Episodes.AddAsync(this.NiblPackage.Episode, cancellationToken);
            this.NiblPackage.Episode.Schedules.Add(await context.Schedules.FindAsync(new object[] { this.scheduleId }, cancellationToken));
            await context.Packages.AddAsync(this.NiblPackage, cancellationToken);
        }
    }
}
