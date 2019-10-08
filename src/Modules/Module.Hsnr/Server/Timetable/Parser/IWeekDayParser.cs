using System.Collections.Generic;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable.Parser
{
    public interface IWeekDayParser :
        IParser<IEnumerable<HtmlNode>, IEnumerable<WeekDay>>
    {
    }
}