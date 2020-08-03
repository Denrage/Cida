using Module.HorribleSubs.Cida.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Module.HorribleSubs.Cida.Services
{
    public class SearchService
    {
        private const string BaseUrl = @"http://xdcc.horriblesubs.info/search.php?t=";

        public async Task<SearchResult[]> SearchAsync(string searchTerm)
        {
            var webClient = new WebClient { Proxy = null };
            var page = await webClient.DownloadStringTaskAsync(new Uri($"{BaseUrl + searchTerm}")).ConfigureAwait(false);
            return this.ParseResultPage(page).ToArray();
        }

        public SearchResult[] Search(string searchTerm)
        {
            var webClient = new WebClient { Proxy = null };
            var page = webClient.DownloadString(new Uri($"{BaseUrl + searchTerm}"));
            return this.ParseResultPage(page).ToArray();
        }

        private IEnumerable<SearchResult> ParseResultPage(string page)
        {
            var lines = page.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Select(
                x =>
                {
                    var line = x.Substring(x.IndexOf('{'), x.IndexOf('}') - x.IndexOf('{') + 1);
                    var data = this.ParseJavaScriptObject(line);
                    return new SearchResult()
                    {
                        BotName = data.Bot,
                        FileSize = data.Size,
                        PackageNumber = data.Number,
                        FileName = data.File
                    };
                });
        }

        private JsonData ParseJavaScriptObject(string line)
        {
            const string bIdentifier = "b:\"";
            const string nIdentifier = "n:";
            const string sIdentifier = "s:";
            const string fIdentifier = "f:\"";
            var regex = new Regex("((b|n|s|f):\\\"?.*?\\\"?(,|\\}))");
            var result = new JsonData();
            foreach (var match in regex.Matches(line))
            {
                var stringMatch = match.ToString();
                stringMatch = stringMatch[0..^1];
                if (stringMatch.Contains(bIdentifier))
                {
                    stringMatch = stringMatch.Replace(bIdentifier, string.Empty);
                    stringMatch = stringMatch[0..^1];
                    result.Bot = stringMatch;
                }
                else if (stringMatch.Contains(nIdentifier))
                {
                    stringMatch = stringMatch.Replace(nIdentifier, string.Empty);
                    result.Number = long.Parse(stringMatch);
                }
                else if (stringMatch.Contains(sIdentifier))
                {
                    stringMatch = stringMatch.Replace(sIdentifier, string.Empty);
                    result.Size = long.Parse(stringMatch);
                }
                else if (stringMatch.Contains(fIdentifier))
                {
                    stringMatch = stringMatch.Replace(fIdentifier, string.Empty);
                    stringMatch = stringMatch[0..^1];
                    result.File = stringMatch;
                }
            }

            return result;
        }

        private class JsonData
        {
            [JsonPropertyName("b")]
            public string Bot { get; set; }

            [JsonPropertyName("n")]
            public long Number { get; set; }

            [JsonPropertyName("s")]
            public long Size { get; set; }

            [JsonPropertyName("f")]
            public string File { get; set; }
        }
    }
}
