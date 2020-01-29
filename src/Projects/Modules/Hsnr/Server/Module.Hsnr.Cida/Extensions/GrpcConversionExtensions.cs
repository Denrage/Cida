using System;
using System.Linq;
using GrpcData = Hsnr;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static GrpcData.TimetableResponse.Types.Timetable ToGrpc(this Timetable.Data.Timetable timetable)
        {
            return new GrpcData.TimetableResponse.Types.Timetable()
            {
                Semester = timetable.Semester.ToGrpc(),
                Type = timetable.Type.ToGrpc(),
                WeekDays = {timetable.WeekDays.Select(x => x.ToGrpc())},
            };
        }

        public static GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay ToGrpc(
            this WeekDay weekDay)
        {
            return new GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay()
            {
                Day = weekDay.Day.ToGrpc(),
                Subjects = {weekDay.Subjects.Select(x => x.ToGrpc())},
            };
        }

        public static GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Subject ToGrpc(
            this Subject subject)
        {
            return new GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Subject()
            {
                End = subject.End,
                Lecturer = subject.Lecturer,
                Name = subject.Name,
                Room = subject.Room,
                Start = subject.Start,
            };
        }

        public static GrpcData.CalendarType ToGrpc(
            this CalendarType calendarType)
        {
            switch (calendarType)
            {
                case CalendarType.Lecturer:
                    return GrpcData.CalendarType.Lecturer;
                case CalendarType.Room:
                    return GrpcData.CalendarType.Room;
                case CalendarType.BranchOfStudy:
                    return GrpcData.CalendarType.BranchOfStudy;
                default:
                    throw new InvalidOperationException("Missing calender type");
            }
        }

        public static GrpcData.SemesterType ToGrpc(
            this SemesterType semesterType)
        {
            switch (semesterType)
            {
                case SemesterType.SummerSemester:
                    return GrpcData.SemesterType.SummerSemester;
                case SemesterType.WinterSemester:
                    return GrpcData.SemesterType.WinterSemester;
                default:
                    throw new InvalidOperationException("Missing semester type");
            }
        }

        public static GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days ToGrpc(
            this Days day)
        {
            switch (day)
            {
                case Days.Monday:
                    return GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Monday;
                case Days.Tuesday:
                    return GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Tuesday;
                case Days.Wednesday:
                    return GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Wednesday;
                case Days.Thursday:
                    return GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Thursday;
                case Days.Friday:
                    return GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Friday;
                case Days.Saturday:
                    return GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Saturday;
                default:
                    throw new InvalidOperationException("Missing day");
            }
        }
        
        public static SemesterType ToModel(
            this GrpcData.SemesterType  semesterType)
        {
            switch (semesterType)
            {
                case GrpcData.SemesterType.SummerSemester:
                    return SemesterType.SummerSemester;
                case GrpcData.SemesterType.WinterSemester:
                    return SemesterType.WinterSemester;
                default:
                    throw new InvalidOperationException("Missing semester type");
            }
        }

        public static Days ToModel(
            this GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days  day)
        {
            switch (day)
            {
                case GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Monday:
                    return Days.Monday;
                case GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Tuesday:
                    return Days.Tuesday;
                case GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Wednesday:
                    return Days.Wednesday;
                case GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Thursday:
                    return Days.Thursday;
                case GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Friday:
                    return Days.Friday;
                case GrpcData.TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days.Saturday:
                    return Days.Saturday;
                default:
                    throw new InvalidOperationException("Missing day");
            }
        }
        
        public static CalendarType ToModel(
            this  GrpcData.CalendarType calendarType)
        {
            switch (calendarType)
            {
                case GrpcData.CalendarType.Lecturer:
                    return CalendarType.Lecturer;
                case GrpcData.CalendarType.Room:
                    return CalendarType.Room;
                case GrpcData.CalendarType.BranchOfStudy:
                    return CalendarType.BranchOfStudy;
                default:
                    throw new InvalidOperationException("Missing calender type");
            }
        }

        public static FormData ToModel(this GrpcData.TimetableRequest request)
        {
            return new FormData()
            {
Calendar = request.Calendar.ToModel(),
Lecturer = request.Lecturer,
Room = request.Room,
Semester = request.Semester.ToModel(),
BranchOfStudy = request.BranchOfStudy,
            };
        }
    }
}