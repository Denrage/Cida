namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class EpisodesResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public EpisodeItem[] Items { get; set; }
    }
}
