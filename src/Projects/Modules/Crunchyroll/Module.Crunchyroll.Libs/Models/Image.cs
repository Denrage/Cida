using System;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models
{
    public class Image
    {
        [JsonPropertyName("thumb_url")]
        public Uri ThumbUrl { get; set; }

        [JsonPropertyName("small_url")]
        public Uri SmallUrl { get; set; }

        [JsonPropertyName("medium_url")]
        public Uri MediumUrl { get; set; }

        [JsonPropertyName("large_url")]
        public Uri LargeUrl { get; set; }

        [JsonPropertyName("full_url")]
        public Uri FullUrl { get; set; }

        [JsonPropertyName("wide_url")]
        public Uri WideUrl { get; set; }

        [JsonPropertyName("widestar_url")]
        public Uri WidestarUrl { get; set; }

        [JsonPropertyName("fwide_url")]
        public Uri FwideUrl { get; set; }

        [JsonPropertyName("fwidestar_url")]
        public Uri FwidestarUrl { get; set; }

        [JsonPropertyName("width")]
        public string Width { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }
    }
}