using System;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Details
{
    public class Data
    {
        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("series_id")]
        public string SeriesId { get; set; }

        [JsonPropertyName("etp_guid")]
        public string EtpGuid { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }

        [JsonPropertyName("landscape_image")]
        public Image LandscapeImage { get; set; }

        [JsonPropertyName("portrait_image")]
        public Image PortraitImage { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}