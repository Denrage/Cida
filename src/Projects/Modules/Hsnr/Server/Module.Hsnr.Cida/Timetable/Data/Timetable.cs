using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Module.Hsnr.Timetable.Data
{
    public class Timetable
    {
        public CalendarType Type { get; }
        
        public SemesterType Semester { get; }
        
        public IReadOnlyList<WeekDay> WeekDays { get; }

        public Timetable(CalendarType type, SemesterType semester, IEnumerable<WeekDay> weekDays)
        {
            this.Type = type;
            this.Semester = semester;
            this.WeekDays = new ReadOnlyCollection<WeekDay>(weekDays.ToList());
        }
    }
}