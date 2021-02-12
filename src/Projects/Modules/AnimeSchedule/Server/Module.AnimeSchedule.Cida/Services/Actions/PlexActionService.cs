using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Ircanime;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Actions
{
    public class PlexActionService : IActionService
    {
        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly ILogger logger;
        private readonly ISettingsService settingsService;

        public PlexActionService(Ircanime.IrcAnimeService.IrcAnimeServiceClient client, ILogger logger, ISettingsService settingsService)
        {
            this.client = client;
            this.logger = logger;
            this.settingsService = settingsService;
        }

        public async Task Execute(AnimeInfoContext animeContext, IAnimeInfo animeInfo, CancellationToken cancellationToken)
        {
            if (animeInfo is NiblAnimeInfo niblAnimeInfo)
            {
                this.logger.Info($"Initiate download on IrcAnime-Module for '{animeInfo.Name}'");
                await this.client.DownloadAsync(new DownloadRequest()
                {
                    DownloadRequest_ = new DownloadRequest.Types.Request()
                    {
                        BotName = "Ginpachi-Sensei",
                        FileName = animeInfo.Name,
                        PackageNumber = (long)niblAnimeInfo.PackageNumber,
                    }
                }, cancellationToken: cancellationToken);

                var checkStatus = true;
                while (checkStatus)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var status = await this.client.DownloadStatusAsync(new DownloadStatusRequest(), cancellationToken: cancellationToken);

                    foreach (var statusElement in status.Status)
                    {
                        if (statusElement.Filename == niblAnimeInfo.Name)
                        {
                            this.logger.Info($"Download status of {niblAnimeInfo.Name} is '{statusElement.Downloaded}'. {statusElement.DownloadedBytes}/{statusElement.Filesize}");
                            if (statusElement.Downloaded)
                            {
                                this.logger.Info($"File '{niblAnimeInfo.Name}' downloaded. Moving to destination folder");
                                await this.Download(niblAnimeInfo, cancellationToken);
                                this.logger.Info($"Finished moving file '{niblAnimeInfo.Name}'. Refreshing Plex Library");
                                await RefreshLibrary(cancellationToken);
                                this.logger.Info($"Library refreshed. Anime '{niblAnimeInfo.Name}' should now be available!");
                                checkStatus = false;
                                break;
                            }
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private async Task RefreshLibrary(CancellationToken cancellationToken)
        {
            var request = WebRequest.Create(await this.PlexRefreshLibrary(cancellationToken));
            request.Method = "GET";

            var oldCallback = ServicePointManager.ServerCertificateValidationCallback;

            ServicePointManager.ServerCertificateValidationCallback =
               new RemoteCertificateValidationCallback(
                    delegate
                    { return true; }
                );

            using var requestRegistry = cancellationToken.Register(() => request.Abort());
            using var response = (HttpWebResponse)await request.GetResponseAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                this.logger.Warn("$Failed to refresh Plex library. Statuscode is '{response.StatusCode}'");
            }

            ServicePointManager.ServerCertificateValidationCallback = oldCallback;
        }

        private async Task<string> PlexRefreshLibrary(CancellationToken cancellationToken) =>
            (await this.settingsService.Get(cancellationToken)).PlexAnimeLibraryUrl + "?X-Plex-Token=" + (await this.settingsService.Get(cancellationToken)).PlexWebToken;

        private async Task Download(NiblAnimeInfo animeInfo, CancellationToken cancellationToken)
        {
            var filename = animeInfo.Name;
            var directory = Path.Combine((await this.settingsService.Get(cancellationToken)).PlexMediaFolder, animeInfo.DestinationFolderName);

            Directory.CreateDirectory(directory);

            var information = await this.client.FileTransferInformationAsync(new FileTransferInformationRequest() { FileName = filename }, cancellationToken: cancellationToken);
            var chunkSize = information.ChunkSize;
            var size = information.Size;
            var sha256 = information.Sha256;

            using var filestream = File.Open(Path.Combine(directory, filename), FileMode.Create, FileAccess.ReadWrite);

            using (var chunkStream = this.client.File(new FileRequest()
            {
                FileName = filename,
                Position = (ulong)filestream.Position
            }, cancellationToken: cancellationToken))
            {
                while (await chunkStream.ResponseStream.MoveNext(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var buffer = new byte[chunkSize];
                    chunkStream.ResponseStream.Current.Chunk.CopyTo(buffer, 0);
                    await filestream.WriteAsync(buffer, 0, (int)chunkStream.ResponseStream.Current.Length, cancellationToken);
                    filestream.Seek((long)chunkStream.ResponseStream.Current.Position, SeekOrigin.Begin);
                }
            }

            using var hash = SHA256.Create();
            filestream.Seek(0, SeekOrigin.Begin);
            var hashString = BitConverter.ToString(hash.ComputeHash(filestream)).Replace("-", "");

            if (hashString != sha256)
            {
                // TODO: DO SOMETHING HERE
                this.logger.Warn($"File '{animeInfo.Name}' has not a correct hash and can be incomplete/altered");
            }
        }
    }
}
