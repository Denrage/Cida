namespace Module.AnimeSchedule.Cida.Models;

public class Episode
{
    public int AnimeId { get; set; }

    public string Name { get; set; }

    public double EpisodeNumber { get; set; }

    public Package Package { get; set; }

    public CrunchyrollEpisode CrunchyrollEpisode { get; set; }

    public AnimeInfo Anime { get; set; }

    public List<Schedule> Schedules {  get; set; }
}
