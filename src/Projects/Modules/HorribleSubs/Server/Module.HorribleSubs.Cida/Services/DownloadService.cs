using Filesystem = Cida.Api.Models.Filesystem;
using Google.Protobuf.WellKnownTypes;
using IrcClient;
using IrcClient.Downloaders;
using Microsoft.EntityFrameworkCore;
using Module.HorribleSubs.Cida.Models;
using Module.HorribleSubs.Cida.Models.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cida.Api;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Immutable;

namespace Module.HorribleSubs.Cida.Services
{
    public partial class DownloadService
    {
        public static string Separator = "/";
        private readonly string tempFolder = Path.Combine(Path.GetTempPath(), "IrcDownloads");
        private readonly IrcClient.Clients.IrcClient ircClient;
        private readonly ConcurrentDictionary<string, CreateDownloaderContext> requestedDownloads;
        private readonly ConcurrentDictionary<string, DownloadProgress> downloadStatus;
        private readonly Func<HorribleSubsDbContext> getContext;
        private readonly IFtpClient ftpClient;
        private readonly Filesystem.Directory downloadDirectory;

        public IReadOnlyDictionary<string, DownloadProgress> CurrentDownloadStatus => this.downloadStatus.ToDictionary(pair => pair.Key, pair => pair.Value);

        public DownloadService(string host, int port, Func<HorribleSubsDbContext> getContext, IFtpClient ftpClient, Filesystem.Directory downloadDirectory)
        {
            string name = "ad_" + Guid.NewGuid();
            this.requestedDownloads = new ConcurrentDictionary<string, CreateDownloaderContext>();
            this.downloadStatus = new ConcurrentDictionary<string, DownloadProgress>();
            this.ircClient = new IrcClient.Clients.IrcClient(host, port, name, name, name, this.tempFolder);
            this.downloadDirectory = downloadDirectory;

            this.ircClient.DownloadRequested += downloader =>
            {
                if (this.requestedDownloads.TryGetValue(downloader.Filename, out var context))
                {
                    context.Downloader = downloader;
                    context.ManualResetEvent.Set();
                }
            };

            this.getContext = getContext;
            this.ftpClient = ftpClient;
        }

        public async Task CreateDownloader(DownloadRequest downloadRequest)
        {
            using (var context = this.getContext())
            {
                if ((await context.Downloads.FindAsync(downloadRequest.FileName)) != null)
                {
                    return;
                }
            }

            if (!this.ircClient.IsConnected)
            {
                this.ircClient.Connect();
            }

            var createDownloaderContext = new CreateDownloaderContext()
            {
                Filename = downloadRequest.FileName,
            };
            var dccDownloaderTask = new Task<DccDownloader>(() =>
            {
                createDownloaderContext.ManualResetEvent.Wait();
                return createDownloaderContext.Downloader;

            }, TaskCreationOptions.LongRunning);

            if (this.requestedDownloads.TryAdd(downloadRequest.FileName, createDownloaderContext))
            {
                this.ircClient.SendMessage($"xdcc send #{downloadRequest.PackageNumber}", downloadRequest.BotName);
            }
            dccDownloaderTask.Start();
            var downloader = await dccDownloaderTask;

            using (var context = this.getContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                await context.Downloads.AddAsync(new Download()
                {
                    Name = downloader.Filename,
                    Size = downloader.Filesize,
                    DownloadStatus = DownloadStatus.Downloading,
                });

                context.ChangeTracker.DetectChanges();
                await context.SaveChangesAsync();
            }


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await this.DownloadFile(downloader));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task DownloadFile(DccDownloader downloader)
        {
            if (this.downloadStatus.TryAdd(downloader.Filename, new DownloadProgress() { Size = downloader.Filesize }))
            {
                downloader.ProgressChanged += (downloadedBytes, size) => UpdateProgress(downloader, downloadedBytes);

                await downloader.StartDownload();

                this.downloadStatus.TryRemove(downloader.Filename, out _);

                using var file = new Filesystem.File(
                    downloader.Filename,
                    this.downloadDirectory,
                    new FileStream(Path.Combine(downloader.TempFolder, downloader.Filename), FileMode.Open, FileAccess.Read));

                using (var context = this.getContext())
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                    var databaseDownloadEntry = await context.Downloads.FindAsync(downloader.Filename);
                    if (databaseDownloadEntry != null)
                    {
                        context.Downloads.Update(databaseDownloadEntry);
                        using var sha256 = SHA256.Create();
                        using var fileStream = await file.GetStreamAsync();

                        databaseDownloadEntry.Sha256 = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "");
                        databaseDownloadEntry.Date = DateTime.Now;
                        databaseDownloadEntry.FtpPath = file.FullPath(Separator);

                        await this.ftpClient.UploadFileAsync(file);
                        databaseDownloadEntry.DownloadStatus = DownloadStatus.Available;

                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private void UpdateProgress(DccDownloader downloader, ulong downloadedBytes)
        {
            if (this.downloadStatus.TryGetValue(downloader.Filename, out var downloadProgress))
            {
                downloadProgress.DownloadedBytes = downloadedBytes;
            }
        }

        private class CreateDownloaderContext
        {
            public string Filename { get; set; }
            public DccDownloader Downloader { get; set; }
            public ManualResetEventSlim ManualResetEvent { get; } = new ManualResetEventSlim(false);
        }

    }
}
