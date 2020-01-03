using System;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Search
{
    public class Data
    {
        [JsonPropertyName("type"), JsonConverter(typeof(TypeEnumConverter))]
        public TypeEnum Type { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("img")] public Uri Img { get; set; }

        [JsonPropertyName("link")] public string Link { get; set; }
    }
}