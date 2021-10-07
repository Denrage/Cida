namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class BrowseResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public SeriesItem[] Items { get; set; }
    }
}
