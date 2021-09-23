namespace Module.AnimeSchedule.Avalonia.Models;

public class Webhook
{
    public ulong Id { get; set; }

    public string Token { get; set; }

    public List<Schedule> Schedules { get; set; }
}
