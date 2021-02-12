using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Module.AnimeSchedule.Cida.Services.Source;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class Schedule
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public List<AnimeInfoContext> Animes { get; set; }

        public TimeSpan Interval { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        // TODO: put that rather into a factory
        public static Schedule FromDb(Models.Database.Schedule schedule, CrunchyrollSourceService crunchyrollSourceService, NiblSourceService niblSourceService, CancellationTokenSource cancellationTokenSource)
        {
            return new Schedule()
            {
                Animes = schedule.AnimeContexts.Select(x => AnimeInfoContext.FromDb(x, crunchyrollSourceService, niblSourceService)).ToList(),
                CancellationTokenSource = cancellationTokenSource,
                Interval = schedule.Interval,
                StartDate = schedule.StartDate,
                Name = schedule.Name,
            };
        }
    }
}
