using System.Text.Json.Serialization;

namespace Module.AnimeSchedule.Cida.Services.Source
{
    public class NiblAnimeResult
    {
        [JsonPropertyName("botId")]
        public int BotId { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("sizekbits")]
        public int Sizekbits { get; set; }

        [JsonPropertyName("episodeNumber")]
        public int EpisodeNumber { get; set; }

        [JsonPropertyName("lastModified")]
        public string LastModified { get; set; }
    }
}
