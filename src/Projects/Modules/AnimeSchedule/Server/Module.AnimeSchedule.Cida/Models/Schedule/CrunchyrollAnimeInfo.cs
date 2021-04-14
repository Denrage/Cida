using System;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class CrunchyrollAnimeInfo : AnimeInfo
    {
        public string SeriesTitle { get; set; }

        public string SeasonTitle { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int SeasonNumber { get; internal set; }
    }
}
