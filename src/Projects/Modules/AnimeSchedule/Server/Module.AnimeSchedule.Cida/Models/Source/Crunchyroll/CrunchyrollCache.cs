using System;
using System.Collections.Concurrent;

namespace Module.AnimeSchedule.Cida.Models.Source.Crunchyroll
{
    public class CrunchyrollCache
    {
        public DateTime LastRefresh { get; set; } = DateTime.MinValue;

        public TimeSpan CacheDuration { get; } = TimeSpan.FromMinutes(10);

        public Cms Cms { get; set; }

        public Token Token { get; set; } = new Token();

        public ConcurrentBag<SeriesItem> Items { get; } = new ConcurrentBag<SeriesItem>();
    }

    public class Token
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromSeconds(1);

        public static implicit operator string(Token token)
        {
            return token.AccessToken;
        }
    }
}
