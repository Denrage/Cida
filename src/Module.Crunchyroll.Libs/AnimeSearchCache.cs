using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Module.Crunchyroll.Libs
{
    public class AnimeSearchCache
    {
        private const string endpoint = "https://crunchyroll.com/ajax/?req=RpcApiSearch_GetSearchCandidates";

        public List<Datum> Items { get; private set; }

        public AnimeSearchCache()
        {
            this.Refresh();
        }

        public void Refresh()
        {
            using var webClient = new WebClient();
            var result = webClient.DownloadString(endpoint);
            var prefixLength = "/*-secure-".Length;
            var suffixLength = "*/".Length;
            result = result.Substring(prefixLength, result.Length - prefixLength - suffixLength);


            var searchData = JsonSerializer.Deserialize<SearchData>(result);
            var test = searchData.Data.Where(x => x.Name.IndexOf("Sword", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            this.Items = new List<Datum>(searchData.Data);
        }

        public IEnumerable<Datum> Search(string searchTerm)
        {
            return this.Items.Where(x => this.LevenshteinDistance(searchTerm, x.Name) < 1);
        }

        private int LevenshteinDistance(string source, string target)
        {
            int n = source.Length;
            int m = target.Length;
            int[,] destination = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; i++)
            {
                destination[i, 0] = i;
            }

            for (int j = 0; j <= m; destination[0, j] = j++)
            {
                destination[0, j] = j;
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    destination[i, j] = Math.Min(
                        Math.Min(destination[i - 1, j] + 1, destination[i, j - 1] + 1),
                        destination[i - 1, j - 1] + cost);
                }
            }

            return destination[n, m] - Math.Abs(n - m);
        }
    }
}

public class SearchData
{
    [JsonPropertyName("result_code")] public long ResultCode { get; set; }

    [JsonPropertyName("message_list")] public object[] MessageList { get; set; }

    [JsonPropertyName("suggested_redirect_url")]
    public object SuggestedRedirectUrl { get; set; }

    [JsonPropertyName("data")] public Datum[] Data { get; set; }

    [JsonPropertyName("exception_class")] public object ExceptionClass { get; set; }

    [JsonPropertyName("exception_error_code")]
    public object ExceptionErrorCode { get; set; }
}

public partial class Datum
{
    [JsonPropertyName("type"), JsonConverter(typeof(TypeEnumConverter))]
    public TypeEnum Type { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("img")] public Uri Img { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }
}

public enum TypeEnum
{
    Allgemein,
    Manga,
    Person,
    Serie,
    Zusammengefasst,
    Unknown
};


internal class TypeEnumConverter : JsonConverter<TypeEnum>
{
    public override TypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();
        if (Enum.TryParse<TypeEnum>(text, out var result))
        {
            return result;
        }

        return TypeEnum.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, TypeEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Enum.GetName(typeof(TypeEnum), value));
    }
}