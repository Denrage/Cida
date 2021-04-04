using Module.IrcAnime.Cida.Models;
using NLog;
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
using System.Threading;
using System.Threading.Tasks;

namespace Module.IrcAnime.Cida.Services
{
    public class SearchService
    {
        private const string BaseUrl = @"https://api.nibl.co.uk/nibl";
        private const string BotPart = "/bots";
        private const string SearchPart = "/search?query=";
        private readonly ILogger logger;
        private List<BotData.Bot> Bots;

        public SearchService(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<SearchResult[]> SearchAsync(string searchTerm, CancellationToken cancellationToken)
        {
            this.logger.Info($"Searching on nibl for '{searchTerm}'");
            await this.EnsureBotListAsync(cancellationToken);
            var webClient = new WebClient { Proxy = null };
            cancellationToken.Register(() => webClient.CancelAsync());
            var page = await webClient.DownloadStringTaskAsync(new Uri($"{BaseUrl + SearchPart + searchTerm}")).ConfigureAwait(false);
            var result = this.Parse(page).ToArray();
            this.logger.Info($"Found {result.Length} items for '{searchTerm}");
            return result;
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
                this.logger.Error($"couldn't parse search results. Status Code: {data.Status}");
                return Array.Empty<SearchResult>();
            }
        }

        public SearchResult[] Search(string searchTerm)
        {
            this.logger.Info($"Searching on nibl for '{searchTerm}'");
            this.EnsureBotList();
            var webClient = new WebClient { Proxy = null };
            var page = webClient.DownloadString(new Uri($"{BaseUrl + SearchPart + searchTerm}"));
            var result = this.Parse(page).ToArray();
            this.logger.Info($"Found {result.Length} items for '{searchTerm}");
            return result;
        }

        private async Task EnsureBotListAsync(CancellationToken cancellationToken)
        {
            this.logger.Info("Ensuring botlist");
            if (this.Bots == null)
            {
                this.logger.Info("bot list out of date, refreshing");
                var webClient = new WebClient { Proxy = null };
                cancellationToken.Register(() => webClient.CancelAsync());
                var page = await webClient.DownloadStringTaskAsync(new Uri($"{BaseUrl + BotPart}")).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<BotData>(page);
                this.Bots = new List<BotData.Bot>();
                if (result.Status == "OK")
                {
                    this.Bots.AddRange(result.Bots);
                    this.logger.Info($"found {this.Bots.Count} bots");
                }
                else
                {
                    this.logger.Error($"Error on refreshing botlist. Result Code: {result.Status}");
                }
            }
        }

        private void EnsureBotList()
        {
            this.logger.Info("Ensuring botlist");
            if (this.Bots == null)
            {
                this.logger.Info("bot list out of date, refreshing");
                var webClient = new WebClient { Proxy = null };
                var page = webClient.DownloadString(new Uri($"{BaseUrl + BotPart}"));
                var result = JsonSerializer.Deserialize<BotData>(page);
                this.Bots = new List<BotData.Bot>();
                if (result.Status == "OK")
                {
                    this.Bots.AddRange(result.Bots);
                    this.logger.Info($"found {this.Bots.Count} bots");
                }
                else
                {
                    this.logger.Error($"Error on refreshing botlist. Result Code: {result.Status}");
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
