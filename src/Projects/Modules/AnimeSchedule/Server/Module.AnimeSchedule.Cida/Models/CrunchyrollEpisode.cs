namespace Module.AnimeSchedule.Cida.Models;

public class CrunchyrollEpisode
{
    public string Title { get; set; }

    public string SeriesTitle { get; set; }

    public string SeasonTitle { get; set; }

    public DateTime ReleaseDate { get; set; }

    public int SeasonNumber { get; set; }

    public Episode Episode { get; set; }

    public string EpisodeName { get; set; }
}
