using System;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Episode
{
    public class StreamInformation
    {
        [JsonPropertyName("quality"), JsonConverter(typeof(QualityEnumConverter))]
        public Quality Quality { get; set; }
        [JsonPropertyName("expires")]
        public DateTime Expires { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}