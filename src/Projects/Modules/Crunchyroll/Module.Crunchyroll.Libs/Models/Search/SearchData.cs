using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Search
{
    public class SearchData
    {
        [JsonPropertyName("result_code")] public long ResultCode { get; set; }

        [JsonPropertyName("message_list")] public object[] MessageList { get; set; }

        [JsonPropertyName("suggested_redirect_url")]
        public object SuggestedRedirectUrl { get; set; }

        [JsonPropertyName("data")] public Data[] Data { get; set; }

        [JsonPropertyName("exception_class")] public object ExceptionClass { get; set; }

        [JsonPropertyName("exception_error_code")]
        public object ExceptionErrorCode { get; set; }
    }
}