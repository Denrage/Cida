using System;
using Module.AnimeSchedule.Cida.Models.Database;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class CrunchyrollAnimeInfo : AnimeInfo
    {
        public string SeriesTitle { get; set; }

        public string SeasonTitle { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int SeasonNumber { get; internal set; }

        public override Episode ToDb()
        {
            return new Episode()
            {
                EpisodeNumber = this.EpisodeNumber,
                Name = this.Name,
            };
        }
    }
}
