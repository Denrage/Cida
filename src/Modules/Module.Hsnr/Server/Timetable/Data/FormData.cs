using System;
using System.Collections.Generic;
using System.Text;

namespace Module.Hsnr.Timetable.Data
{
    public class FormData
    {
        public SemesterType Semester { get; set; }

        public CalendarType Calendar { get; set; }

        public string Lecturer { get; set; }

        public string Room { get; set; }

        public string BranchOfStudy { get; set; }

        public string ToParameters()
        {
            var parameters = new Dictionary<string, string>()
            {
                {"KalenderOK", CalendarTypeToParameterName(this.Calendar)},
                {"Lage", SemesterTypeToParameterName(this.Semester) },
                {"RadioButton_Dozent", this.Lecturer },
                {"RadioButton_Raum", this.Room },
                {"RadioButton_SR", this.BranchOfStudy },
                {"clear", "false" },
                {"first_element", "false" },
                {"fkt", CalendarTypeToParameterName(this.Calendar) },
                {"mode", CalendarTypeToParameterName(this.Calendar) },
                {"SR", this.BranchOfStudy },
            };

            var stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.Key).Append("=").Append(parameter.Value).Append("&");
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }

        private static string CalendarTypeToParameterName(CalendarType calendarType)
        {
            switch (calendarType)
            {
                case CalendarType.BranchOfStudy:
                    return "SR";
                case CalendarType.Room:
                    return "Raum";
                case CalendarType.Lecturer:
                    return "Dozent";
                default:
                    throw new NotImplementedException("Missing calendar type");
            }
        }

        private static string SemesterTypeToParameterName(SemesterType semesterType)
        {
            switch (semesterType)
            {
                case SemesterType.SummerSemester:
                    return "SS";
                case SemesterType.WinterSemester:
                    return "WS";
                default:
                    throw new NotImplementedException("Missing semester type");
            }
        }
    }
}