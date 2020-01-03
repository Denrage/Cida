using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Details
{
    public class Details
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        [JsonPropertyName("debug")]
        public Debug Debug { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}


