using Discord;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Services.Source;

public class CrunchyrollEpisodeContext : INotifyable, IDatabaseSavable
{
    private readonly Anilist4Net.Client anilistClient;
    private readonly int scheduleId;

    public CrunchyrollEpisode CrunchyrollEpisode { get; }

    public CrunchyrollEpisodeContext(CrunchyrollEpisode crunchyrollEpisode, Anilist4Net.Client client, int scheduleId)
    {
        this.CrunchyrollEpisode = crunchyrollEpisode;
        this.anilistClient = client;
        this.scheduleId = scheduleId;
    }

    public async Task<Embed> CreateEmbed()
    {
        var animeInfo = await this.anilistClient.GetMediaById((int)this.CrunchyrollEpisode.Episode.AnimeId);
        var embedBuilder = new Discord.EmbedBuilder();
        embedBuilder
            .WithTitle($"Episode {this.CrunchyrollEpisode.Episode.EpisodeNumber}: {this.CrunchyrollEpisode.Title}")
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
            episode.Schedules.Add(await context.Schedules.FindAsync(new object[] { this.scheduleId }, cancellationToken));
            context.Episodes.Update(episode);
        }
        else
        {
            this.CrunchyrollEpisode.Episode.Schedules.Add(await context.Schedules.FindAsync(new object[] { this.scheduleId }, cancellationToken));
            await context.Episodes.AddAsync(this.CrunchyrollEpisode.Episode, cancellationToken);
            await context.CrunchyrollEpisodes.AddAsync(this.CrunchyrollEpisode, cancellationToken);
        }
    }
}
