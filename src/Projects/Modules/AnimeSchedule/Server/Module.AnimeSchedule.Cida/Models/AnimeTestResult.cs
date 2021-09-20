using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AnimeSchedule.Cida.Models
{
    public class AnimeTestResult
    {
        public string EpisodeName {  get; set; }

        public double EpisodeNumber {  get; set; }

        public string SeriesTitle { get; set; } = string.Empty;

        public string SeasonTitle { get; set; } = string.Empty;
    }
}
