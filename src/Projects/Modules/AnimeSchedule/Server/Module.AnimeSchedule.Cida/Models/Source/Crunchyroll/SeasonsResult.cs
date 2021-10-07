using Module.AnimeSchedule.Cida.Services.Source;

namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class SeasonsResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public SeasonItem[] Items { get; set; }
    }
}
