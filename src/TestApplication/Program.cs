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
            var ircAnimeClient = new Ircanime.IrcAnimeService.IrcAnimeServiceClient(channel);
            //var scheduleResult = client.CreateSchedule(new Animeschedule.CreateScheduleRequest()
            //{
            //    Interval = Duration.FromTimeSpan(TimeSpan.FromMinutes(10)),
            //    StartDate = Timestamp.FromDateTime(DateTime.UtcNow - TimeSpan.FromHours(1)),
            //    Name = "Hello Worl1d",
            //});
            //Console.WriteLine($"ScheduleId: {scheduleResult.ScheduleId} - Result: {scheduleResult.CreateResult}");

            //var webhookResult = client.CreateDiscordWebhook(new Animeschedule.CreateDiscordWebhookRequest()
            //{
            //    WebhookId = 886540609903554572,
            //    WebhookToken = "ZVyy2kri80eigqbRmTg9s0qCS_S_i94zb9km9DLt0iF-zYrx4mY7yId0m6S89_2CIHDT",
            //});
            //Console.WriteLine($"Result: {webhookResult.CreateResult}");

            //var webhookAssignResult = client.AssignWebhookToSchedule(new Animeschedule.AssignWebhookToScheduleRequest()
            //{
            //    WebhookId = 886540609903554572,
            //    ScheduleId = 1,
            //});
            //Console.WriteLine($"Result: {webhookAssignResult.AssignResult}");

            //var animeInfoResult = client.CreateAnime(new Animeschedule.CreateAnimeRequest()
            //{
            //    Id = 126546,
            //    Identifier = "Seirei Gensouki",
            //    Type = Animeschedule.CreateAnimeRequest.Types.AnimeInfoType.Nibl,
            //    Filter = "1080",
            //    Folder = "Seirei Gensouki",
            //});
            //Console.WriteLine($"Result: {animeInfoResult.CreateResult}");

            //var animeInfoAssignResult = client.AssignAnimeInfoToSchedule(new Animeschedule.AssignAnimeInfoToScheduleRequest()
            //{
            //    AnimeId = 126546,
            //    ScheduleId = 1,
            //});
            //Console.WriteLine($"Result: {animeInfoAssignResult.AssignResult}");

            var schedules = client.GetSchedules(new Animeschedule.GetSchedulesRequest());

            Console.WriteLine(string.Join(",", schedules.Schedules.Select(x => x.State)));

            client.ForceRunSchedule(new Animeschedule.ForceRunScheduleRequest()
            {
                ScheduleId = 1,
            });

            schedules = client.GetSchedules(new Animeschedule.GetSchedulesRequest());

            Console.WriteLine(string.Join(",", schedules.Schedules.Select(x => x.State)));


            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
