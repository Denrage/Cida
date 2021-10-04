using System.Security.Cryptography;
using Ircanime;
using Module.AnimeSchedule.Cida.Interfaces;
using Module.AnimeSchedule.Cida.Models;
using NLog;

namespace Module.AnimeSchedule.Cida.Services.Actions;

public class DownloadActionService : IMultiActionService
{
    private readonly IrcAnimeService.IrcAnimeServiceClient client;
    private readonly ILogger logger;
    private readonly ISettingsService settingsService;
    private readonly DiscordClient discordClient;
    private readonly Func<AnimeScheduleDbContext> getContext;

    public DownloadActionService(IrcAnimeService.IrcAnimeServiceClient client, ILogger logger, ISettingsService settingsService, DiscordClient discordClient, Func<AnimeScheduleDbContext> getContext)
    {
        this.client = client;
        this.logger = logger;
        this.settingsService = settingsService;
        this.discordClient = discordClient;
        this.getContext = getContext;
    }

    public async Task Execute(IEnumerable<IActionable> actionables, int scheduleId, CancellationToken cancellationToken)
    {
        var downloadAnimes = actionables.Where(x => x is IDownloadable).Cast<IDownloadable>().Where(x => !x.AlreadyProcessed);
        if (downloadAnimes.Any())
        {
            var informations = new List<DownloadInformation>();
            foreach (var item in downloadAnimes)
            {
                informations.Add(await item.GetDownloadInformation(this.getContext, cancellationToken));
            }

            this.logger.Info($"Initiate download on IrcAnime-Module for '{string.Join(",", informations.Select(x => x.FileName))}'");
            var downloadRequest = new DownloadRequest();
            downloadRequest.DownloadRequest_.AddRange(informations.Select(x => new DownloadRequest.Types.Request()
            {
                BotName = x.Bot,
                FileName = x.FileName,
                PackageNumber = (long)x.PackageNumber,
            }));

            await this.client.DownloadAsync(downloadRequest, cancellationToken: cancellationToken);
            var toCheck = new Dictionary<string, (DownloadInformation DownloadInformation, Ircanime.DownloadStatusResponse.Types.DownloadStatus Status)>();

            foreach (var information in informations)
            {
                toCheck.Add(information.FileName, (information, null));
            }

            var clients = await this.discordClient.GetClients(scheduleId);


            while (toCheck.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var status = await this.client.DownloadStatusAsync(new DownloadStatusRequest(), cancellationToken: cancellationToken);

                foreach (var statusElement in status.Status)
                {
                    if (toCheck.ContainsKey(statusElement.Filename))
                    {
                        toCheck[statusElement.Filename] = (toCheck[statusElement.Filename].DownloadInformation, statusElement);

                        this.logger.Info($"Download status of {statusElement.Filename} is '{statusElement.Downloaded}'. {statusElement.DownloadedBytes}/{statusElement.Filesize}");
                        if (statusElement.Downloaded)
                        {
                            this.logger.Info($"File '{statusElement.Filename}' downloaded. Moving to destination folder");
                            await this.Download(toCheck[statusElement.Filename].DownloadInformation, cancellationToken);
                            this.logger.Info($"Finished moving file '{statusElement.Filename}'.");
                            toCheck.Remove(statusElement.Filename);
                            foreach (var client in clients)
                            {
                                await client.SendMessageAsync($"{statusElement.Filename} is now available!");
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    private async Task Download(DownloadInformation animeInfo, CancellationToken cancellationToken)
    {
        var filename = animeInfo.FileName;
        var directory = Path.Combine((await this.settingsService.Get(cancellationToken)).MediaFolder, animeInfo.DestinationFolderName);

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
            this.logger.Warn($"File '{animeInfo.FileName}' has not a correct hash and can be incomplete/altered");
        }

        this.logger.Info($"Downloaded anime '{animeInfo.AnimeName}' to '{directory}'");
    }
}
