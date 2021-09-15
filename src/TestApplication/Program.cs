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
            var request = new DownloadRequest();
            request.DownloadRequest_.Add(new DownloadRequest.Types.Request()
            {
                BotName = "CR-HOLLAND|NEW",
                FileName = "[SubsPlease] Seirei Gensouki - 01 (1080p) [EC64AE1A].mkv",
                PackageNumber = 16776,
            });
            request.DownloadRequest_.Add(new DownloadRequest.Types.Request()
            {
                BotName = "CR-HOLLAND|NEW",
                FileName = "[SubsPlease] Seirei Gensouki - 02 (1080p) [487207A9].mkv",
                PackageNumber = 16907,
            });
            request.DownloadRequest_.Add(new DownloadRequest.Types.Request()
            {
                BotName = "CR-HOLLAND|NEW",
                FileName = "[SubsPlease] Seirei Gensouki - 03 (1080p) [B897760D].mkv",
                PackageNumber = 17046,
            });
            request.DownloadRequest_.Add(new DownloadRequest.Types.Request()
            {
                BotName = "CR-HOLLAND|NEW",
                FileName = "[SubsPlease] Seirei Gensouki - 04 (1080p) [A27AA2EF].mkv",
                PackageNumber = 17177,
            });
            request.DownloadRequest_.Add(new DownloadRequest.Types.Request()
            {
                BotName = "CR-HOLLAND|NEW",
                FileName = "[SubsPlease] Seirei Gensouki - 05 (1080p) [6ABAB137].mkv",
                PackageNumber = 17337,
            });
            var result = ircAnimeClient.Download(request);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
