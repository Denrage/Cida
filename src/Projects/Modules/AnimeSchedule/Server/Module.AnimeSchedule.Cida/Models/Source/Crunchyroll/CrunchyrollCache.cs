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
}
