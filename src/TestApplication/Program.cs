using Grpc.Core;
using Horriblesubs;
using System;
using System.Net;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:31564", ChannelCredentials.Insecure);
            var client = new HorribleSubsService.HorribleSubsServiceClient(channel);
            var results = client.Search(new SearchRequest()
            {
                SearchTerm = "Sword Art Online",
            });

            client.Download(new DownloadRequest()
            {
                DownloadRequest_ = new DownloadRequest.Types.Request()
                {
                    BotName = "Ginpachi-Sensei",
                    FileName = "[tlacatlc6] Sword Art Online Alternative - Gun Gale Online Ad4 (BD-Raw 1920x1080 HEVC AAC) [7D954099].mkv",
                    PackageNumber = 553
                }
            });

            //foreach(var result in results.SearchResults)
            //{
            //    Console.WriteLine($"Botname:{result.BotName};Filename:{result.FileName};Filesize:{result.FileSize};PackageNumber:{result.PackageNumber}");
            //}
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
        }
    }
}