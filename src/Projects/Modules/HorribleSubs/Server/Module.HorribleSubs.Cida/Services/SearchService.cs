using Module.HorribleSubs.Cida.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                    var data = JsonSerializer.Deserialize<JsonData>(line);
                    return new SearchResult()
                    {
                        BotName = data.Bot,
                        FileSize = data.Size,
                        PackageNumber = data.Number,
                        FileName = data.File
                    };
                });
        }

        private class JsonData
        {
            [JsonPropertyName("b")]
            public string Bot { get; set; }

            [JsonPropertyName("n")]
            public int Number { get; set; }

            [JsonPropertyName("s")]
            public int Size { get; set; }

            [JsonPropertyName("f")]
            public string File { get; set; }
        }
    }
}
