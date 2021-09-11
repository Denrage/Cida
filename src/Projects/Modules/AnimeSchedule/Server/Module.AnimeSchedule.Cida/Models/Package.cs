namespace Module.AnimeSchedule.Cida.Models;

public class Package
{
    public string EpisodeName { get; set; }
    public ulong PackageNumber { get; set; }
    public string BotName { get; set; }
    public Episode Episode { get; set; }
}
