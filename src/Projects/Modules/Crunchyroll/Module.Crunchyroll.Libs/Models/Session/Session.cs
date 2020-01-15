using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Session
{
    public class Session
    {
        [JsonPropertyName("session_id")] public string SessionId { get; set; }

        [JsonPropertyName("country_code")] public string CountryCode { get; set; }

        [JsonPropertyName("ip")] public string Ip { get; set; }

        [JsonPropertyName("device_type")] public string DeviceType { get; set; }

        [JsonPropertyName("device_id")] public string DeviceId { get; set; }

        [JsonPropertyName("user")] public object User { get; set; }

        [JsonPropertyName("auth")] public object Auth { get; set; }

        [JsonPropertyName("expires")] public object Expires { get; set; }

        [JsonPropertyName("version")] public object Version { get; set; }

        [JsonPropertyName("ops")] public object[] Ops { get; set; }

        [JsonPropertyName("connectivity_type")] public string ConnectivityType { get; set; }

        [JsonPropertyName("debug")] public Dictionary<string, double> Debug { get; set; }
    }
}