using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;
using Microsoft.EntityFrameworkCore;
using Module.AnimeSchedule.Cida.Interfaces;
using NLog;

namespace Module.AnimeSchedule.Cida.Services;

public class DiscordClient
{
    private readonly SemaphoreSlim settingsSemaphore = new SemaphoreSlim(1);
    private readonly Func<AnimeScheduleDbContext> getContext;

    protected ILogger Logger { get; private set; }

    private Dictionary<uint, IEnumerable<ulong>> scheduleClientMapping = new Dictionary<uint, IEnumerable<ulong>>();

    private Dictionary<ulong, DiscordWebhookClient> clients = new Dictionary<ulong, DiscordWebhookClient>();

    public DiscordClient(ILogger logger, Func<AnimeScheduleDbContext> getContext)
    {
        this.Logger = logger;
        this.getContext = getContext;
    }

    public async Task<IEnumerable<DiscordWebhookClient>> GetClients(uint scheduleId)
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

            throw new InvalidOperationException("Clients for schedule '{scheduleId}' not found!");
        }
        finally
        {
            this.settingsSemaphore.Release();
        }
    }

    public async Task InitializeClients(CancellationToken cancellationToken)
    {
        await this.settingsSemaphore.WaitAsync();

        foreach (var item in this.clients)
        {
            item.Value.Dispose();
        }

        this.clients.Clear();
        this.scheduleClientMapping.Clear();

        try
        {
            using var context = this.getContext();
            var schedules = await context.DiscordSchedules.Include(x => x.Webhooks).ToArrayAsync(cancellationToken);
            var webhooks = await context.DiscordWebhooks.Include(x => x.Schedules).ToArrayAsync(cancellationToken);

            foreach (var item in schedules)
            {
                this.scheduleClientMapping[item.ScheduleId] = item.Webhooks.Select(x => x.WebhookId).ToArray();
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
