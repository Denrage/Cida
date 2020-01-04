using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Module.Hsnr.Timetable.Data
{
    public class WeekDay
    {
        public Days Day { get; }

        public IEnumerable<Subject> Subjects { get; }

        public WeekDay(Days day, IEnumerable<Subject> subjects)
        {
            this.Day = day;
            this.Subjects = subjects;
        }
    }
}