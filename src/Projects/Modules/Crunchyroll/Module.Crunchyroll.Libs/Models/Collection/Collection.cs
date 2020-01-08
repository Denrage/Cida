using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Collection
{
    public class Collection
    {
        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("collection_id")]
        public string CollectionId { get; set; }

        [JsonPropertyName("etp_guid")]
        public string EtpGuid { get; set; }

        [JsonPropertyName("series_id")]
        public string SeriesId { get; set; }

        [JsonPropertyName("series_etp_guid")]
        public string SeriesEtpGuid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }

        [JsonPropertyName("season")]
        public string Season { get; set; }

        [JsonPropertyName("complete")]
        public bool Complete { get; set; }

        [JsonPropertyName("landscape_image")]
        public Image LandscapeImage { get; set; }

        [JsonPropertyName("portrait_image")]
        public Image PortraitImage { get; set; }

        [JsonPropertyName("availability_notes")]
        public string AvailabilityNotes { get; set; }

        [JsonPropertyName("created")]
        public string Created { get; set; }
    }
}