using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Session
{
    public class Result
    {
        [JsonPropertyName("data")] public Session Session { get; set; }

        [JsonPropertyName("error")] public bool Error { get; set; }

        [JsonPropertyName("code")] public string Code { get; set; }
    }
}