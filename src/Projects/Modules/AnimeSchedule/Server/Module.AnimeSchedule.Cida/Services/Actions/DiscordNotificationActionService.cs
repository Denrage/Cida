using Discord;
using Module.AnimeSchedule.Cida.Interfaces;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Actions;
public class DiscordNotificationActionService : IActionService
{
    private readonly ILogger logger;
    private readonly ISettingsService settingsService;
    private readonly DiscordClient client;

    public DiscordNotificationActionService(ILogger logger, ISettingsService settingsService, DiscordClient client)
    {
        this.logger = logger;
        this.settingsService = settingsService;
        this.client = client;
    }

    public async Task Execute(IActionable actionable, int scheduleId, CancellationToken cancellationToken)
    {
        if (actionable is INotifyable notifyableAnime)
        {
            Embed embed = await notifyableAnime.CreateEmbed();
            var clients = await this.client.GetClients(scheduleId);

            foreach (var item in clients)
            {
                await item.SendMessageAsync(embeds: new[] { embed });
            }
            System.Diagnostics.Debug.WriteLine("EMBED SEND");
        }
    }
}
