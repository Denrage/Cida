using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable.Parser
{
    public class WeekDayParser : IWeekDayParser
    {
        private readonly ITimetableTimeParser timeParser;

        public WeekDayParser(ITimetableTimeParser timeParser)
        {
            this.timeParser = timeParser;
        }
        // Parsing will easily break on a website change
        public IEnumerable<WeekDay> Parse(IEnumerable<HtmlNode> rows)
        {
            var htmlNodes = rows.ToList();
            var times = this.timeParser.Parse(htmlNodes.First());
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

            var result = new List<WeekDay>();
            for (var i = 0; i < listOfDays.Count; i++)
            {
                result.Add(new WeekDay(listOfDays[i], (Days) i, times));
            }

            return result;
        }
    }

    public interface IWeekDayParser :
        IParser<IEnumerable<HtmlNode>, IEnumerable<WeekDay>>
    {
    }

    public interface IParser<in TIn, out TOut>
    {
        TOut Parse(TIn value);
    }
}