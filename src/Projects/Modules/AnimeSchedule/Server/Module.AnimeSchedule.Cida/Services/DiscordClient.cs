using Discord.Webhook;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Module.AnimeSchedule.Cida.Services;

public class DiscordClient
{
    private readonly SemaphoreSlim settingsSemaphore = new SemaphoreSlim(1);
    private readonly Func<AnimeScheduleDbContext> getContext;

    protected ILogger Logger { get; private set; }

    private Dictionary<int, IEnumerable<ulong>> scheduleClientMapping = new();

    private Dictionary<ulong, DiscordWebhookClient> clients = new();

    public DiscordClient(ILogger logger, Func<AnimeScheduleDbContext> getContext)
    {
        this.Logger = logger;
        this.getContext = getContext;
    }

    public async Task<IEnumerable<DiscordWebhookClient>> GetClients(int scheduleId)
    {
        await this.settingsSemaphore.WaitAsync();

        try
        {
            if (this.scheduleClientMapping.TryGetValue(scheduleId, out var clients))
            {
                var result = new List<DiscordWebhookClient>();
                foreach (var item in clients)
                {
                    result.Add(this.clients[item]);
                }

                return result;
            }

            this.Logger.Info($"Clients for schedule '{scheduleId}' not found!");

            return Enumerable.Empty<DiscordWebhookClient>();
        }
        finally
        {
            this.settingsSemaphore.Release();
        }
    }

    public bool TestWebhook(ulong webhook, string token)
    {
        try
        {
            var client = new DiscordWebhookClient(webhook, token);
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }

        return true;
    }

    public async Task InitializeClients(CancellationToken cancellationToken)
    {
        await this.settingsSemaphore.WaitAsync();
        try
        {
            foreach (var item in this.clients)
            {
                item.Value.Dispose();
            }

            this.clients.Clear();
            this.scheduleClientMapping.Clear();

            using var context = this.getContext();
            var schedules = await context.Schedules.Include(x => x.DiscordWebhooks).ToArrayAsync(cancellationToken);
            var webhooks = await context.DiscordWebhooks.Include(x => x.Schedules).ToArrayAsync(cancellationToken);

            foreach (var item in schedules)
            {
                this.scheduleClientMapping[item.Id] = item.DiscordWebhooks.Select(x => x.WebhookId).ToArray();
            }

            foreach (var item in webhooks)
            {
                this.clients[item.WebhookId] = new DiscordWebhookClient(item.WebhookId, item.WebhookToken);
            }
        }
        finally
        {
            this.settingsSemaphore.Release();
        }
    }
}
