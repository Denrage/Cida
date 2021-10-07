namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class SeasonItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("series_id")]
        public string SeriesId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }
    }
}
