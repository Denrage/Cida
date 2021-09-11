﻿namespace Module.AnimeSchedule.Cida.Models;

public class Schedule
{
    public uint Id { get; set; }

    public string Name { get; set; }

    public TimeSpan Interval { get; set; }

    public DateTime StartDate { get; set; }

    public List<AnimeInfo> Animes { get; set; }

    public List<DiscordWebhook> DiscordWebhooks { get; set; }
}
