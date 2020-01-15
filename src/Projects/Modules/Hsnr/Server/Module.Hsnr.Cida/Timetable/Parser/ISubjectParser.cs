using System.Collections.Generic;
using HtmlAgilityPack;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable.Parser
{
    public interface ISubjectParser 
        : IParser<(IEnumerable<HtmlNode> Rows, TimetableTime[] Times), IEnumerable<Subject>>
    {
    }
}