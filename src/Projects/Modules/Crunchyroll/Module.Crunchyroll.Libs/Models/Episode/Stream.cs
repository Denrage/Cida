using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Episode
{
    public class StreamData
    {
        [JsonPropertyName("stream_data")]
        public Stream Stream { get; set; }
    }
    public class Stream
    {
        [JsonPropertyName("hardsub_lang")]
        public string SubtitleLanguage { get; set; }
        [JsonPropertyName("audio_lang")]
        public string AudioLanguage { get; set; }
        [JsonPropertyName("format")]
        public string Format { get; set; }
        [JsonPropertyName("streams")]
        public StreamInformation[] Streams { get; set; }
    }
}
