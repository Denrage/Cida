using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Episode
{
    public class Result
    {
        [JsonPropertyName("data")]
        public Episode[] Data { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}
