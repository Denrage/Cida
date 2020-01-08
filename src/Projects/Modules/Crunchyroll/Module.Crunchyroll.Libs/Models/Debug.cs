using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models
{
    public class Debug
    {
        [JsonPropertyName("to_data")]
        public double ToData { get; set; }
    }
}