using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;
using Module.Hsnr.Timetable.Parser;

namespace Module.Hsnr.Timetable
{
    public class TimetableService : ITimetableService
    {
        private readonly IWeekDayParser weekDayParser;
        private readonly TimetableConnector connector;

        public SiteCache SiteCache { get; }

        public TimetableService(IWeekDayParser weekDayParser)
        {
            this.weekDayParser = weekDayParser;
            this.connector = new TimetableConnector();
            this.SiteCache = new SiteCache(this.connector);
        }

        public async Task<Data.Timetable> GetTimetableAsync(FormData formData)
        {
            var result = await this.connector.PostDataAsync(formData);
            var document = new HtmlDocument();
            document.LoadHtml(result);
            var element = document.GetElementbyId("tempdata0").ParentNode;

            var rows = element.ChildNodes;
            var weekDays = this.weekDayParser.Parse(rows);
            return new Data.Timetable(formData.Calendar, formData.Semester, weekDays);
        }
    }

    public interface ITimetableService
    {
        Task<Data.Timetable> GetTimetableAsync(FormData formData);
    }

    public class MockTimetableService : ITimetableService
    {
        public List<Subject> GetMockSubjects()
        {
            var subjList = new List<Subject>();
            for (int i = 8; i < 20; i += 1)
            {
                var subj = new Subject
                {
                    Start = i,
                    End = i + 2,
                    Lecturer = "Re",
                    Name = "VSY V",
                    Room = $"B111"
                };
                subjList.Add(subj);
            }
            return subjList;
        }

        public async Task<Data.Timetable> GetTimetableAsync(FormData formData)
        {
            return await Task.FromResult(new Data.Timetable(CalendarType.BranchOfStudy, SemesterType.WinterSemester, new[] {
                new WeekDay(Days.Monday, GetMockSubjects()),
                new WeekDay(Days.Tuesday, GetMockSubjects()),
                new WeekDay(Days.Wednesday, GetMockSubjects()),
                new WeekDay(Days.Thursday, GetMockSubjects()),
                new WeekDay(Days.Friday, GetMockSubjects()),
                new WeekDay(Days.Saturday, new List<Subject>()),
            }));
        }
    }
}