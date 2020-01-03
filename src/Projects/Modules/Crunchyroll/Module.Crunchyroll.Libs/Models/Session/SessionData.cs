using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Session
{
    public class SessionData
    {
        [JsonPropertyName("data")] public Data Data { get; set; }

        [JsonPropertyName("error")] public bool Error { get; set; }

        [JsonPropertyName("code")] public string Code { get; set; }
    }
}