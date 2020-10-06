using Module.IrcAnime.Cida.Models;
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

namespace Module.IrcAnime.Cida.Services
{
    public class SearchService
    {
        private const string BaseUrl = @"https://api.nibl.co.uk/nibl";
        private const string BotPart = "/bots";
        private const string SearchPart = "/search?query=";
        private List<BotData.Bot> Bots;

        public async Task<SearchResult[]> SearchAsync(string searchTerm)
        {
            await this.EnsureBotListAsync();
            var webClient = new WebClient { Proxy = null };
            var page = await webClient.DownloadStringTaskAsync(new Uri($"{BaseUrl + SearchPart + searchTerm}")).ConfigureAwait(false);
            return this.Parse(page).ToArray();
        }

        private SearchResult[] Parse(string json)
        {
            var data = JsonSerializer.Deserialize<JsonData>(json);

            if (data.Status == "OK")
            {
                string getBotName(long id)
                {
                    var result = this.Bots.FirstOrDefault(x => x.Id == id);
                    return result == null ? id.ToString() : result.Name;
                }

                return data.Episodes.Select(x => new SearchResult()
                {
                    BotName = getBotName(x.BotId),
                    FileName = x.Name,
                    FileSize = x.Sizekbits,
                    PackageNumber = x.Number,
                }).ToArray();
            }
            else
            {
                return Array.Empty<SearchResult>();
            }
        }

        public SearchResult[] Search(string searchTerm)
        {
            this.EnsureBotList();
            var webClient = new WebClient { Proxy = null };
            var page = webClient.DownloadString(new Uri($"{BaseUrl + SearchPart + searchTerm}"));
            return this.Parse(page);
        }

        private async Task EnsureBotListAsync()
        {
            if (this.Bots == null)
            {
                var webClient = new WebClient { Proxy = null };
                var page = await webClient.DownloadStringTaskAsync(new Uri($"{BaseUrl + BotPart}")).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<BotData>(page);
                this.Bots = new List<BotData.Bot>();
                if (result.Status == "OK")
                {
                    this.Bots.AddRange(result.Bots);
                }
            }
        }

        private void EnsureBotList()
        {
            if (this.Bots == null)
            {
                var webClient = new WebClient { Proxy = null };
                var page = webClient.DownloadString(new Uri($"{BaseUrl + BotPart}"));
                var result = JsonSerializer.Deserialize<BotData>(page);
                this.Bots = new List<BotData.Bot>();
                if (result.Status == "OK")
                {
                    this.Bots.AddRange(result.Bots);
                }
            }
        }

        private class BotData
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("content")]
            public Bot[] Bots { get; set; }

            public class Bot
            {
                [JsonPropertyName("id")]
                public long Id { get; set; }

                [JsonPropertyName("name")]
                public string Name { get; set; }

                [JsonPropertyName("owner")]
                public string Owner { get; set; }

                [JsonPropertyName("lastProcessed")]
                public string LastProcessed { get; set; }

                [JsonPropertyName("batchEnable")]
                public long BatchEnable { get; set; }

                [JsonPropertyName("packSize")]
                public long PackSize { get; set; }
            }
        }

        private class JsonData
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("content")]
            public Episode[] Episodes { get; set; }

            public class Episode
            {
                [JsonPropertyName("botId")]
                public long BotId { get; set; }

                [JsonPropertyName("number")]
                public long Number { get; set; }

                [JsonPropertyName("name")]
                public string Name { get; set; }

                [JsonPropertyName("size")]
                public string Size { get; set; }

                [JsonPropertyName("sizekbits")]
                public long Sizekbits { get; set; }

                [JsonPropertyName("episodeNumber")]
                public long EpisodeNumber { get; set; }

                [JsonPropertyName("lastModified")]
                public string LastModified { get; set; }
            }
        }
    }
}
