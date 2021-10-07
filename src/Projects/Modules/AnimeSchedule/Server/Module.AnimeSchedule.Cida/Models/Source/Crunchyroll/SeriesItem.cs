using System;

namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class SeriesItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("last_public")]
        public DateTime LastPublic { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
