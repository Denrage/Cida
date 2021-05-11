﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models.Schedule;
using Module.AnimeSchedule.Cida.Models.Source.Crunchyroll;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Source
{
    // API is undocumented and completely reverse engineered, therefor
    // continously subject to change
    public class CrunchyrollSourceService : ISourceService
    {
        private const string BaseUrl = "https://beta-api.crunchyroll.com/";
        private const string TokenEndpoint = "auth/v1/token";
        private const string BrowseEndpoint = "content/v1/browse";
        private const string IndexEndpoint = "index/v2";
        private const string SeasonsEndpoint = "cms/v2{0}/seasons";
        private const string EpisodesEndpoint = "cms/v2{0}/episodes";

        private readonly CrunchyrollCache cache = new CrunchyrollCache();
        private readonly SemaphoreSlim cacheSemaphore = new SemaphoreSlim(1);
        private readonly HttpClient httpClient = new HttpClient();
        private readonly ILogger logger;

        public CrunchyrollSourceService(ILogger logger)
        {
            this.logger = logger;
        }

        private async Task<bool> RefreshCache(CancellationToken cancellationToken)
        {
            await this.cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (DateTime.Now - this.cache.LastRefresh > this.cache.CacheDuration)
                {
                    this.logger.Info("Cache not up to date. Refreshing ...");

                    if (DateTime.Now - this.cache.Token.LastUpdated > this.cache.Token.ExpiresIn)
                    {
                        this.logger.Info("Token expired, requesting new one");
                        if (string.IsNullOrEmpty(this.cache.Token.RefreshToken))
                        {
                            var token = await this.GetTokenAsync(cancellationToken);
                            this.cache.Token.AccessToken = token.AccessToken;
                            this.cache.Token.RefreshToken = token.RefreshToken;
                            this.cache.Token.ExpiresIn = TimeSpan.FromSeconds(token.ExpiresIn) - TimeSpan.FromSeconds(10);
                            this.cache.Token.LastUpdated = DateTime.Now;
                        }
                        else
                        {
                            var token = await this.GetTokenAsync(cancellationToken, this.cache.Token.RefreshToken);
                            this.cache.Token.AccessToken = token.AccessToken;
                            this.cache.Token.RefreshToken = token.RefreshToken;
                            this.cache.Token.ExpiresIn = TimeSpan.FromSeconds(token.ExpiresIn);
                            this.cache.Token.LastUpdated = DateTime.Now;
                        }
                    }

                    this.cache.Cms = await this.GetIndexAsync(this.cache.Token, cancellationToken);

                    var series = await this.GetNewlyAddedSeriesAsync(this.cache.Token, cancellationToken);

                    this.cache.Items.Clear();

                    foreach (var item in series)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        this.cache.Items.Add(item);
                    }

                    this.cache.LastRefresh = DateTime.Now;

                    this.logger.Info("Cache refreshed");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to refresh cache!");
                return false;
            }
            finally
            {
                this.cacheSemaphore.Release();
            }
            return true;
        }

        public async Task<IEnumerable<IAnimeInfo>> GetNewEpisodes(AnimeInfoContext context, CancellationToken cancellationToken)
        {
            if (!await this.RefreshCache(cancellationToken))
            {
                this.logger.Warn("Couldn't refresh cache, retrying in the next cycle");
                return Enumerable.Empty<IAnimeInfo>();
            }

            this.logger.Info($"Checking for new episodes for '{context.Identifier}'");
            var temp = new List<CrunchyrollAnimeInfo>();
            foreach (var item in this.cache.Items.Where(x => context.Identifier.ToUpper().Contains(x.Title.ToUpper())))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var seasonId = await this.GetRelevantSeasonIdAsync(this.cache.Token, item.Id, this.cache.Cms, context.Identifier, cancellationToken);
                var episodes = await this.GetEpisodesAsync(this.cache.Token, seasonId, this.cache.Cms, cancellationToken);

                foreach (var episode in episodes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    temp.Add(new CrunchyrollAnimeInfo()
                    {
                        EpisodeNumber = episode.EpisodeNumber,
                        MyAnimeListId = context.MyAnimeListId,
                        Name = episode.Title,
                        SeriesTitle = episode.SeriesTitle,
                        SeasonTitle = episode.SeasonTitle,
                        ReleaseDate = episode.ReleaseDate,
                        SeasonNumber = episode.SeasonNumber,
                    });
                }
            }

            var newEpisodes = temp.Where(x => !context.Episodes.Select(y => y.EpisodeNumber).Contains(x.EpisodeNumber)).ToArray();
            this.logger.Info($"'{newEpisodes.Length}' new episodes found for '{context.Identifier}'");

            return newEpisodes;
        }

        private async Task<TokenResult> GetTokenAsync(CancellationToken cancellationToken, string refreshToken = null)
        {
            Dictionary<string, string> values;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                values = new Dictionary<string, string>()
                {
                    {"refresh_token", refreshToken },
                    {"grant_type", "refresh_token"},
                };
            }
            else
            {
                values = new Dictionary<string, string>()
                {
                    {"grant_type", "client_id" },
                };
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(BaseUrl + TokenEndpoint),
                Content = new FormUrlEncodedContent(values)
            };

            request.Headers.Add("accept-encoding", "gzip");
            request.Headers.Add("authorization", "Basic bHJzZXNfMjV6aDNranRhNnI5cjk6YWZEWGlEWjhhNkVsektRcEY2bzJwc0xiY0czaVpGSHM=");
            request.Headers.Add("user-agent", "Crunchyroll/3.4.1 Android/10 okhttp/4.8.1");
            request.Headers.Add("etp-anonymous-id", "fad42e89-c67c-4f09-9fc5-c2279f9ff8a4");

            var result = await this.RequestAsync<TokenResult>(request, cancellationToken);

            if (result != null)
            {
                return result;
            }

            return null;
        }

        private async Task<IEnumerable<EpisodeItem>> GetEpisodesAsync(string token, string seasonId, Cms cms, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(BaseUrl + string.Format(EpisodesEndpoint, cms.Bucket) + $"?Policy={cms.Policy}&Signature={cms.Signature}&Key-Pair-Id={cms.KeyPairId}&locale=en-US&season_id={seasonId}"),
            };

            request.Headers.Add("accept-encoding", "gzip");

            var result = await this.RequestAsync<EpisodesResult>(request, cancellationToken);

            if (result != null)
            {
                return result.Items;
            }

            return Enumerable.Empty<EpisodeItem>();
        }

        private async Task<string> GetRelevantSeasonIdAsync(string token, string seriesId, Cms cms, string identifier, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(BaseUrl + string.Format(SeasonsEndpoint, cms.Bucket) + $"?Policy={cms.Policy}&Signature={cms.Signature}&Key-Pair-Id={cms.KeyPairId}&locale=en-US&series_id={seriesId}"),
            };

            request.Headers.Add("accept-encoding", "gzip");

            var result = await this.RequestAsync<SeasonsResult>(request, cancellationToken);

            if (result != null)
            {
                return result.Items.Where(x => x.Title.ToUpper() == identifier.ToUpper()).OrderByDescending(x => x.SeasonNumber).First().Id;
            }

            return string.Empty;
        }

        private async Task<IEnumerable<SeriesItem>> GetNewlyAddedSeriesAsync(string token, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(BaseUrl + BrowseEndpoint + "?n=50&locale=en-US&sort_by=newly_added"),
            };

            request.Headers.Add("accept-encoding", "gzip");
            request.Headers.Add("authorization", "Bearer " + token);

            var result = await this.RequestAsync<BrowseResult>(request, cancellationToken);

            if (result != null)
            {
                return result.Items;
            }

            return Enumerable.Empty<SeriesItem>();
        }

        private async Task<Cms> GetIndexAsync(string token, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(BaseUrl + IndexEndpoint),
            };

            request.Headers.Add("accept-encoding", "gzip");
            request.Headers.Add("authorization", "Bearer " + token);

            var result = await this.RequestAsync<IndexResult>(request, cancellationToken);

            if (result != null)
            {
                return result.Cms;
            }

            return null;
        }

        private async Task<T> RequestAsync<T>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
            where T : class
        {
            var response = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                using var content = new GZipStream(await response.Content.ReadAsStreamAsync(cancellationToken), CompressionMode.Decompress);
                using var contentReader = new StreamReader(content);
                var stringContent = await contentReader.ReadToEndAsync();
                return System.Text.Json.JsonSerializer.Deserialize<T>(stringContent);
            }
            else
            {
                this.logger.Warn($"Request '{httpRequestMessage.RequestUri}' was not successfull. StatusCode was '{response.StatusCode}'");
                return null;
            }
        }
    }
}