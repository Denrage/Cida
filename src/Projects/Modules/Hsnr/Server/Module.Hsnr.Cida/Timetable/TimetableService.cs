using System.Threading.Tasks;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;
using Module.Hsnr.Timetable.Parser;

namespace Module.Hsnr.Timetable
{
    public class TimetableService
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
}