using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable.Parser
{
    public class WeekDayParser : IWeekDayParser
    {
        private readonly ITimetableTimeParser timeParser;
        private readonly ISubjectParser subjectParser;

        public WeekDayParser(ITimetableTimeParser timeParser, ISubjectParser subjectParser)
        {
            this.timeParser = timeParser;
            this.subjectParser = subjectParser;
        }
        // Parsing will easily break on a website change
        public IEnumerable<WeekDay> Parse(IEnumerable<HtmlNode> rows)
        {
            var htmlNodes = rows.ToList();
            var times = this.timeParser.Parse(htmlNodes.First()).ToArray();
            var listOfDays = new List<List<HtmlNode>>
            {
                new List<HtmlNode>()
            };
            
            var currentWeekDay = 0;
            
            // First row are times
            foreach (var row in htmlNodes.Skip(1))
            {
                if (listOfDays.Count - 1 != currentWeekDay)
                {
                    listOfDays.Add(new List<HtmlNode>());
                }

                if (row.OriginalName == "tr")
                {
                    if (row.ChildNodes.Count > 1)
                    {
                        listOfDays[currentWeekDay].Add(row);
                    }
                }
                else if (row.OriginalName == "input")
                {
                    currentWeekDay++;
                }
            }

            return listOfDays
                .Select((t, i) => new WeekDay((Days) i, this.subjectParser.Parse((t, times))));
        }
    }
}