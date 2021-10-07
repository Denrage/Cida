namespace Module.AnimeSchedule.Cida.Models;

public class DiscordWebhook
{
    public ulong WebhookId { get; set; }

    public string WebhookToken { get; set; }

    public List<Schedule> Schedules { get; set; }
}
