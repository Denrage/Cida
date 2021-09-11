using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Utils;
using Ircanime;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace TestApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:31564", ChannelCredentials.Insecure);
            var client = new Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient(channel);
            var result = client.CreateSchedule(new Animeschedule.CreateScheduleRequest()
            {
                Interval = Duration.FromTimeSpan(TimeSpan.FromMinutes(10)),
                StartDate = Timestamp.FromDateTime(DateTime.UtcNow - TimeSpan.FromHours(1)),
                Name = "Hello Worl1d",
            });
            Console.WriteLine($"ScheduleId: {result.ScheduleId} - Result: {result.CreateResult}");
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
