using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using JikanDotNet;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Actions
{
    public class DiscordNotificationActionService : IActionService
    {
        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly IJikan jikan;
        private readonly SemaphoreSlim settingsSemaphore = new SemaphoreSlim(1);

        private DiscordWebhookClient webhookClient;

        public DiscordNotificationActionService(ILogger logger, ISettingsService settingsService)
        {
            this.logger = logger;
            this.settingsService = settingsService;
            jikan = new Jikan();
            this.settingsService.OnSettingsChanged += async () => await InitializeClient();
        }

        private async Task InitializeClient()
        {
            await this.settingsSemaphore.WaitAsync();

            try
            {
                var settings = await this.settingsService.Get(default);
                this.webhookClient = new DiscordWebhookClient(ulong.Parse(settings.DiscordWebhookId), settings.DiscordWebhookToken);
            }
            finally
            {
                this.settingsSemaphore.Release();
            }
        }

        public async Task Execute(AnimeInfoContext animeContext, IAnimeInfo animeInfo, CancellationToken cancellationToken)
        {
            if (this.webhookClient is null)
            {
                await this.InitializeClient();
            }

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

            await this.webhookClient.SendMessageAsync(embeds: new[] { embed });
        }

        private async Task<Embed> CreateCrunchyrollEmbed(CrunchyrollAnimeInfo anime, CancellationToken cancellationToken)
        {
            this.logger.Info($"Creating Crunchyroll embed of '{anime.Name}'");
            var myAnimelistInfo = await this.jikan.GetAnime((long)anime.MyAnimeListId);
            var embedBuilder = new Discord.EmbedBuilder();
            embedBuilder
                .WithFooter("Crunchyroll")
                .WithDescription("Episode: " + anime.EpisodeNumber.ToString())
                .WithTitle(myAnimelistInfo.Title)
                .WithImageUrl(myAnimelistInfo.ImageURL);

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
