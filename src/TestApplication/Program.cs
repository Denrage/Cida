using System;
using System.Text;
using Grpc.Core;
using Hsnr;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var channel = new Channel("127.0.0.2:31565", ChannelCredentials.Insecure);
//            var client = new Cida.CidaApiService.CidaApiServiceClient(channel);
//            Console.WriteLine(client.Version(new Cida.VersionRequest()).Version);
            var client2 = new HsnrTimetableService.HsnrTimetableServiceClient(channel);
            var response = client2.Timetable(new TimetableRequest()
            {
                Calendar = CalendarType.BranchOfStudy,
                BranchOfStudy = "KBI5",
                Semester = SemesterType.WinterSemester,
            });

            foreach (var weekDay in response.Result.WeekDays)
            {
                Console.WriteLine(weekDay.Day);
                    var stringBuilder = new StringBuilder();
                foreach (var subject in weekDay.Subjects)
                {
                    stringBuilder.Append(subject.Start).Append(" - ").Append(subject.End).Append("\t")
                        .Append(subject.Name).Append("\t").Append(subject.Room).Append("\t")
                        .AppendLine(subject.Lecturer);
                }
                Console.WriteLine(stringBuilder.ToString() + "\n\n");
            }
            

            Console.ReadKey();
            channel.ShutdownAsync().Wait();
        }
    }
}