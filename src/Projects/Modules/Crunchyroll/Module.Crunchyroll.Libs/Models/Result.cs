using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models
{
    public class Result<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("debug")]
        public Debug Debug { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}