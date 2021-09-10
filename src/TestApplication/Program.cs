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
            var filename = "[HorribleSubs] Sword Art Online - Alicization - War of Underworld - 16 [1080p].mkv";
            var channel = new Channel("127.0.0.1:31564", ChannelCredentials.Insecure);
            var client = new IrcAnimeService.IrcAnimeServiceClient(channel);
            //var results = client.Search(new SearchRequest()
            //{
            //    SearchTerm = "Sword Art Online",
            //});

            //foreach (var result in results.SearchResults)
            //{
            //Console.WriteLine($"Botname:{result.BotName};Filename:{result.FileName};Filesize:{result.FileSize};PackageNumber:{result.PackageNumber}");
            //}

            //client.Download(new DownloadRequest()
            //{
            //    DownloadRequest_ = new DownloadRequest.Types.Request()
            //    {
            //        BotName = "CR-ARUTHA|NEW",
            //        FileName = filename,
            //        PackageNumber = 10734
            //    }
            //});

            //while (true)
            //{
            //    var downloadStatus = client.DownloadStatus(new DownloadStatusRequest());
            //    foreach (var status in downloadStatus.Status.Where(x => !x.Downloaded))
            //    {
            //        Console.WriteLine($"{status.Filename}: {((double)status.DownloadedBytes / (double)status.Filesize) * 100}%");
            //    }
            //    Thread.Sleep(3000);
            //}

            client.Download(new DownloadRequest()
            {
                DownloadRequest_ = { new DownloadRequest.Types.Request()
                {
                    BotName = "Ginpachi-Sensei",
                    PackageNumber = 7,
                    FileName = "[Beatrice-Raws] Kaguya-sama wa Kokurasetai (Creditless ED 03 video storyboard) [BDRip 1920x1080 HEVC FLAC].mkv"
                } }
            });
        }
    }
}
