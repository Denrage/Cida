using Grpc.Core;
using Grpc.Core.Utils;
using Horriblesubs;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Grpc.Core;
using Hsnr;
﻿using Grpc.Core;
using Hsnr;
using System;
using System.Net;
using Crunchyroll;
using System.Threading.Tasks;

namespace TestApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = new Channel("ipv4:127.0.0.1:31564", ChannelCredentials.Insecure);
            var client = new HsnrTimetableService.HsnrTimetableServiceClient(channel);
            var client2 = new CrunchyrollService.CrunchyrollServiceClient(channel);
            Console.WriteLine("Ready");
            while (true)
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        return;
                    case ConsoleKey.T:
                        Console.WriteLine("Getting timetable");
                        var timetable = client.Timetable(new TimetableRequest()
                        {
                            Calendar = CalendarType.BranchOfStudy,
                            BranchOfStudy = "KBI5",
                            Semester = SemesterType.WinterSemester,
                        });
                        foreach (var day in timetable.Result.WeekDays)
                        {
                            Console.WriteLine(Enum.GetName(typeof(TimetableResponse.Types.Timetable.Types.WeekDay.Types.Days), day.Day));
                            foreach (var subject in day.Subjects)
                            {
                                Console.WriteLine($"{subject.Start}-{subject.End}: {subject.Name} - {subject.Room} - {subject.Lecturer}");
                            }
                        }
                        break;
                    case ConsoleKey.A:
                        Console.WriteLine("Searching for \"sword\"");
                        var searchResult = await client2.SearchAsync(new Crunchyroll.SearchRequest() { SearchTerm = "sword" });
                        Console.WriteLine($"Found {searchResult.Items.Count} results");
                        break;
                    default:
                        break;
                }
            }
            Console.WriteLine("Done");
        }
    }
}