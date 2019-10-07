using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Module.Hsnr.Timetable.Data
{
    public class WeekDay
    {
        public Days Day { get; }

        public List<Subject> Subjects { get; }

        public WeekDay(IEnumerable<HtmlNode> tableRows, Days day, List<(int, int)> times)
        {
            this.Day = day;
            this.Subjects = new List<Subject>();
            this.ParseRows(tableRows, times);
        }

        private void ParseRows(IEnumerable<HtmlNode> tableRows, List<(int, int)> times)
        {
            foreach (var row in tableRows)
            {
                var currentTime = 0;
                foreach (var child in row.ChildNodes)
                {
                    var firstChildOfChild = child.ChildNodes.FirstOrDefault(x => x.OriginalName == "a");
                    var duration = child.GetAttributeValue("colspan", 1);
                    if (firstChildOfChild != null)
                    {
                        var start = times[currentTime].Item1;
                        var end = times[currentTime + duration - 1].Item2;
                    }

                    if (!child.Id.Contains("WT"))
                    {
                        currentTime += duration;
                    }
                }
            }
        }
    }
}