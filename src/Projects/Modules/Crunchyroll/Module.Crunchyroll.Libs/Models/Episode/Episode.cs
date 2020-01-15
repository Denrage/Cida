using System;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Episode
{
    public class Episode
    {
        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("media_id")]
        public string MediaId { get; set; }

        [JsonPropertyName("etp_guid")]
        public string EtpGuid { get; set; }

        [JsonPropertyName("collection_id")]
        public string CollectionId { get; set; }

        [JsonPropertyName("collection_etp_guid")]
        public string CollectionEtpGuid { get; set; }

        [JsonPropertyName("series_id")]
        public string SeriesId { get; set; }

        [JsonPropertyName("series_etp_guid")]
        public string SeriesEtpGuid { get; set; }

        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }

        [JsonPropertyName("episode_number")]
        public string EpisodeNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("screenshot_image")]
        public Image ScreenshotImage { get; set; }

        [JsonPropertyName("bif_url")]
        public Uri BifUrl { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("clip")]
        public bool Clip { get; set; }

        [JsonPropertyName("available")]
        public bool Available { get; set; }

        [JsonPropertyName("premium_available")]
        public bool PremiumAvailable { get; set; }

        [JsonPropertyName("free_available")]
        public bool FreeAvailable { get; set; }

        [JsonPropertyName("available_time")]
        public string AvailableTime { get; set; }

        [JsonPropertyName("unavailable_time")]
        public string UnavailableTime { get; set; }

        [JsonPropertyName("premium_available_time")]
        public string PremiumAvailableTime { get; set; }

        [JsonPropertyName("premium_unavailable_time")]
        public string PremiumUnavailableTime { get; set; }

        [JsonPropertyName("free_available_time")]
        public string FreeAvailableTime { get; set; }

        [JsonPropertyName("free_unavailable_time")]
        public string FreeUnavailableTime { get; set; }

        [JsonPropertyName("availability_notes")]
        public string AvailabilityNotes { get; set; }

        [JsonPropertyName("created")]
        public string Created { get; set; }

        [JsonPropertyName("playhead")]
        public long Playhead { get; set; }
    }
}