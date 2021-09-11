using Anilist4Net;
using Discord;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Services.Source;

public class NiblEpisodeContext : INotifyable, IDownloadable, IDatabaseSavable
{
    private readonly Client anilistClient;

    public Package NiblPackage { get; }

    public bool AlreadyProcessed { get; set; } = false;

    public NiblEpisodeContext(Package package, Anilist4Net.Client anilistClient)
    {
        this.NiblPackage = package;
        this.anilistClient = anilistClient;
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

    public async Task<DownloadInformation> GetDownloadInformation()
    {
        var animeInfo = await this.anilistClient.GetMediaById((int)this.NiblPackage.Episode.AnimeId);
        return new DownloadInformation()
        {
            AnimeName = animeInfo.RomajiTitle,
            Bot = this.NiblPackage.BotName,
            DestinationFolderName = this.NiblPackage.Episode.Anime.AnimeFolder.FolderName,
            FileName = this.NiblPackage.EpisodeName,
            PackageNumber = this.NiblPackage.PackageNumber,
        };
    }

    public async Task SaveToDatabase(AnimeScheduleDbContext context, CancellationToken cancellationToken)
    {
        var episode = await context.Episodes.FindAsync(this.NiblPackage.Episode.Name);
        if (episode != null)
        {
            episode.Created = DateTime.Now;
            context.Episodes.Update(episode);
        }
        else
        {
            await context.Episodes.AddAsync(this.NiblPackage.Episode, cancellationToken);
            await context.Packages.AddAsync(this.NiblPackage, cancellationToken);
        }
    }
}
