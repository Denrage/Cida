using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable
{
    public class TimetableService
    {
        private readonly TimetableConnector connector;

        public SiteCache SiteCache { get; }

        public TimetableService()
        {
            this.connector = new TimetableConnector();
            this.SiteCache = new SiteCache(this.connector);
        }

        public Data.Timetable GetTimetable(FormData formData)
        {
            var result = this.connector.PostData(formData);
            var document = new HtmlDocument();
            document.LoadHtml(result);
            var element = document.GetElementbyId("tempdata0").ParentNode;

            var rows = element.ChildNodes;
            var times = this.ParseTimes(rows[0]);
            var weekDays = this.ParseWeekdays(rows.Skip(1), times);
            return new Data.Timetable(formData.Calendar, formData.Semester, weekDays);
        }

        private IList<WeekDay> ParseWeekdays(IEnumerable<HtmlNode> weekdayRows, List<(int, int)> times)
        {
            var listOfDays = new List<List<HtmlNode>>
            {
                new List<HtmlNode>()
            };
            var currentWeekDay = 0;
            foreach (var row in weekdayRows)
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

        private List<(int start, int end)> ParseTimes(HtmlNode tableRow)
        {
            var timings = new List<(int start, int end)>();
            foreach (var child in tableRow.ChildNodes)
            {
                if (string.IsNullOrWhiteSpace(child.InnerHtml))
                {
                    continue;
                }

                var splittedTimings = child.InnerText.Split('-');
                timings.Add((int.Parse(splittedTimings[0]), int.Parse(splittedTimings[1])));
            }

            return timings;
        }
    }
}