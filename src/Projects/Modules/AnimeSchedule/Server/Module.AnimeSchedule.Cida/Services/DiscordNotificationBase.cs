using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;
using Module.AnimeSchedule.Cida.Interfaces;
using NLog;

namespace Module.AnimeSchedule.Cida.Services
{
    public class DiscordClient
    {
        private readonly ISettingsService settingsService;
        private readonly SemaphoreSlim settingsSemaphore = new SemaphoreSlim(1);

        protected ILogger Logger { get; private set; }

        public DiscordWebhookClient WebhookClient { get; private set; }

        public DiscordClient(ILogger logger, ISettingsService settingsService)
        {
            this.Logger = logger;
            this.settingsService = settingsService;
            this.settingsService.OnSettingsChanged += async () => await InitializeClient();
        }

        public async Task InitializeClient()
        {
            await this.settingsSemaphore.WaitAsync();

            try
            {
                var settings = await this.settingsService.Get(default);
                this.WebhookClient = new DiscordWebhookClient(ulong.Parse(settings.DiscordWebhookId), settings.DiscordWebhookToken);
            }
            finally
            {
                this.settingsSemaphore.Release();
            }
        }
    }
}
