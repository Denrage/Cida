using Grpc.Core;
using Grpc.Core.Utils;
using Horriblesubs;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = "[HorribleSubs] Yahari Ore no Seishun Love Come wa Machigatteiru Kan - 04 [1080p].mkv";
            var channel = new Channel("127.0.0.1:31564", ChannelCredentials.Insecure);
            var client = new HorribleSubsService.HorribleSubsServiceClient(channel);
            var results = client.Search(new SearchRequest()
            {
                SearchTerm = "Sword Art Online",
            });

            foreach (var result in results.SearchResults)
            {
                Console.WriteLine($"Botname:{result.BotName};Filename:{result.FileName};Filesize:{result.FileSize};PackageNumber:{result.PackageNumber}");
            }

            client.Download(new DownloadRequest()
            {
                DownloadRequest_ = new DownloadRequest.Types.Request()
                {
                    BotName = "CR-HOLLAND|NEW",
                    FileName = filename,
                    PackageNumber = 10692
                }
            });

            var isDownloaded = false;
            while (!isDownloaded)
            {
                var downloadStatus = client.DownloadStatus(new DownloadStatusRequest());
                isDownloaded = downloadStatus.Status.FirstOrDefault(x => x.Filename == filename).Downloaded;
                Thread.Sleep(1000);
            }

            var information = client.FileTransferInformation(new FileTransferInformationRequest()
            {
                FileName = filename
            });

            Console.WriteLine($"ChunkSize: {information.ChunkSize} Size: {information.Size} SHA256: {information.Sha256}");

            var response = client.File(new FileRequest()
            {
                FileName = filename
            });

            using var fileStream = new FileStream(@"D:\Animes\temp\SAO.mkv", FileMode.Create);

            response.ResponseStream.ForEachAsync(async fileResponse =>
            {
                await fileStream.WriteAsync(fileResponse.Chunk.ToByteArray(), 0, (int)fileResponse.Length);
                fileStream.Seek((long)fileResponse.Position, SeekOrigin.Begin);
            }).GetAwaiter().GetResult();
        }
    }
}