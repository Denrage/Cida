namespace Module.AnimeSchedule.Avalonia.Models;

public class Schedule
{
    public string Name { get; set; } = string.Empty;

    public int ScheduleId { get; set; }

    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(30);

    public DateTime StartDate { get; set; } = DateTime.Now;

    public ScheduleState State { get; set; }

    public List<AnimeInfo> Animes { get; set; }
}
