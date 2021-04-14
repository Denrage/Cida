using System;

namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class EpisodeItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("series_title")]
        public string SeriesTitle { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("season_id")]
        public string SeasonId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("season_title")]
        public string SeasonTitle { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("episode")]
        public string Episode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("episode_number")]
        public int EpisodeNumber { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("episode_air_date")]
        public DateTime ReleaseDate { get; set; }
    }
}
