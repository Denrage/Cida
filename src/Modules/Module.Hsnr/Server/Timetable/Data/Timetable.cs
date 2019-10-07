using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Module.Hsnr.Timetable.Data
{
    public class Timetable
    {
        public CalendarType Type { get; }
        
        public SemesterType Semester { get; }
        
        private readonly IList<WeekDay> weekDays;
        public IReadOnlyList<WeekDay> WeekDays { get; }

        public Timetable(CalendarType type, SemesterType semester,IList<WeekDay> weekDays)
        {
            this.Type = type;
            this.Semester = semester;
            this.weekDays = weekDays;
            this.WeekDays = new ReadOnlyCollection<WeekDay>(this.weekDays);
        }
    }
}