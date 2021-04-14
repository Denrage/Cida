using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using JikanDotNet;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Actions
{
    public class DiscordNotificationActionService : IActionService
    {
        private readonly IJikan jikan;
        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly DiscordClient client;

        public DiscordNotificationActionService(ILogger logger, ISettingsService settingsService, DiscordClient client)
        {
            jikan = new Jikan();
            this.logger = logger;
            this.settingsService = settingsService;
            this.client = client;
        }

        public async Task Execute(AnimeInfoContext animeContext, IAnimeInfo animeInfo, CancellationToken cancellationToken)
        {
            Embed embed;
            switch (animeInfo)
            {
                case CrunchyrollAnimeInfo crunchyrollAnimeInfo:
                    embed = await CreateCrunchyrollEmbed(crunchyrollAnimeInfo, cancellationToken);
                    break;

                case NiblAnimeInfo niblAnimeInfo:
                    embed = await CreateNiblEmbed(niblAnimeInfo, cancellationToken);
                    break;

                default:
                    this.logger.Warn($"Unexpected IAnimeInfo type '{animeInfo.GetType().FullName}'");
                    return;
            }

            await this.client.WebhookClient.SendMessageAsync(embeds: new[] { embed });
        }

        private async Task<Embed> CreateCrunchyrollEmbed(CrunchyrollAnimeInfo anime, CancellationToken cancellationToken)
        {
            this.logger.Info($"Creating Crunchyroll embed of '{anime.Name}'");
            var myAnimelistInfo = await this.jikan.GetAnime((long)anime.MyAnimeListId);
            var embedBuilder = new Discord.EmbedBuilder();
            embedBuilder
                .WithTitle($"Episode {anime.EpisodeNumber}: {anime.Name}")
                .WithAuthor($"{myAnimelistInfo.Title}", iconUrl: "https://upload.wikimedia.org/wikipedia/commons/0/08/Crunchyroll_Logo.png")
                .WithColor(16711680)
                .WithFooter(anime.ReleaseDate.ToString("r"))
                .WithImageUrl(myAnimelistInfo.ImageURL)
                .WithFields(
                    new[]
                    {
                        new EmbedFieldBuilder().WithName($"Season {anime.SeasonNumber}").WithValue(anime.SeasonTitle).WithIsInline(true),
                    });

            return embedBuilder.Build();
        }

        private async Task<Embed> CreateNiblEmbed(NiblAnimeInfo anime, CancellationToken cancellationToken)
        {
            this.logger.Info($"Creating Nibl embed of '{anime.Name}'");
            var myAnimelistInfo = await this.jikan.GetAnime((long)anime.MyAnimeListId);
            var embedBuilder = new Discord.EmbedBuilder();
            embedBuilder
                .WithFooter("Ginpachi-Sensei")
                .WithDescription("Episode: " + anime.EpisodeNumber.ToString())
                .WithTitle(myAnimelistInfo.Title)
                .WithImageUrl(myAnimelistInfo.ImageURL)
                .AddField("Original Name", anime.Name);

            return embedBuilder.Build();
        }
    }
}
