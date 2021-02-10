using System;
using System.Collections.Generic;

namespace Module.AnimeSchedule.Cida.Models.Database
{
    public class Schedule
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public TimeSpan Interval { get; set; }

        public DateTime StartDate { get; set; }

        public List<AnimeContext> AnimeContexts { get; set; }
    }
}