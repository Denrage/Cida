using Discord;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Services.Source;

public class CrunchyrollEpisodeContext : INotifyable, IDatabaseSavable
{
    private readonly Anilist4Net.Client anilistClient;

    public CrunchyrollEpisode CrunchyrollEpisode { get; }

    public CrunchyrollEpisodeContext(CrunchyrollEpisode crunchyrollEpisode, Anilist4Net.Client client)
    {
        this.CrunchyrollEpisode = crunchyrollEpisode;
        this.anilistClient = client;
    }

    public async Task<Embed> CreateEmbed()
    {
        var animeInfo = await this.anilistClient.GetMediaById((int)this.CrunchyrollEpisode.Episode.AnimeId);
        var embedBuilder = new Discord.EmbedBuilder();
        embedBuilder
            .WithTitle($"Episode {this.CrunchyrollEpisode.Episode.EpisodeNumber}: {this.CrunchyrollEpisode.Episode.Name}")
            .WithAuthor($"{animeInfo.RomajiTitle}", iconUrl: "https://upload.wikimedia.org/wikipedia/commons/0/08/Crunchyroll_Logo.png")
            .WithColor(16711680)
            .WithFooter(this.CrunchyrollEpisode.ReleaseDate.ToString("r"))
            .WithImageUrl(animeInfo.CoverImageLarge)
            .WithFields(
                new[]
                {
                        new EmbedFieldBuilder().WithName($"Season {this.CrunchyrollEpisode.SeasonNumber}").WithValue(this.CrunchyrollEpisode.SeasonTitle).WithIsInline(true),
                });

        return embedBuilder.Build();
    }

    public async Task SaveToDatabase(AnimeScheduleDbContext context, CancellationToken cancellationToken)
    {
        var episode = await context.Episodes.FindAsync(this.CrunchyrollEpisode.Episode.Name);
        if (episode != null)
        {
            episode.Created = DateTime.Now;
            context.Episodes.Update(episode);
        }
        else
        {
            await context.Episodes.AddAsync(this.CrunchyrollEpisode.Episode, cancellationToken);
            await context.CrunchyrollEpisodes.AddAsync(this.CrunchyrollEpisode, cancellationToken);
        }
    }
}
