using System;
using System.Net;
using Grpc.Core;
using Hsnr;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // var channel = new Channel("ipv4:127.0.0.1:31564,127.0.0.2:31564", ChannelCredentials.Insecure, new[] { new ChannelOption("grpc.lb_policy_name", "round_robin") });
            // var client = new HsnrTimetableService.HsnrTimetableServiceClient(channel);
            // Console.WriteLine("Ready");
            // while (true)
            // {
            //     var key = Console.ReadKey();
            //     switch (key.Key)
            //     {
            //         case ConsoleKey.Q:
            //             return;
            //         case ConsoleKey.T:
            //             Console.WriteLine("Getting timetable");
            //             var timetable = client.Timetable(new TimetableRequest()
            //             {
            //                 Calendar = CalendarType.BranchOfStudy,
            //                 BranchOfStudy = "KBI5",
            //                 Semester = SemesterType.WinterSemester,
            //             });
            //             foreach (var day in timetable.Result.WeekDays)
            //             {
            //                 Console.WriteLine(Enum.GetName(typeof(TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days), day.Day));
            //                 foreach (var subject in day.Subjects)
            //                 {
            //                     Console.WriteLine($"{subject.Start}-{subject.End}: {subject.Name} - {subject.Room} - {subject.Lecturer}");
            //                 }
            //             }
            //             break;
            //         default:
            //             break;
            //     }
            // }
            // Console.WriteLine("Done");
            var result = (FtpWebRequest)WebRequest.Create($"ftp://127.0.0.1:21");
            result.Credentials = new NetworkCredential("Cida", "cida");
            result.Method = WebRequestMethods.Ftp.ListDirectory;
            using var response = (FtpWebResponse) result.GetResponse();
            Console.WriteLine(response.StatusCode);
        }
    }
}