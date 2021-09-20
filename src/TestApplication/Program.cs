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

            var result = client.TestAnime(new Animeschedule.TestAnimeRequest()
            {
                Id = 108465,
                Identifier = "Mushoku Tensei",
                Filter = "",
                Type = Animeschedule.AnimeInfoType.Nibl,
            });

            foreach (var item in result.Animes)
            {
                Console.WriteLine($"Name: '{item.EpisodeName}' \t Number: '{item.EpisodeNumber}' \t Season: '{item.SeasonTitle}' \t Series: '{item.SeriesTitle}'");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
