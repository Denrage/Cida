using Filesystem = Cida.Api.Models.Filesystem;
using Google.Protobuf.WellKnownTypes;
using IrcClient;
using IrcClient.Connections;
using IrcClient.Handlers;
using IrcClient.Commands;
using IrcClient.Clients;
using Microsoft.EntityFrameworkCore;
using Module.IrcAnime.Cida.Models;
using Module.IrcAnime.Cida.Models.Database;
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
using NLog;

namespace Module.IrcAnime.Cida.Services
{
    public partial class DownloadService
    {
        public static string Separator = "/";
        private readonly string tempFolder = Path.Combine(Path.GetTempPath(), "IrcDownloads");
        private readonly IrcConnection ircConnection;
        private readonly ConcurrentDictionary<string, CreateDownloaderContext> requestedDownloads;
        private readonly ConcurrentDictionary<string, DownloadProgress> downloadStatus;
        private readonly Func<IrcAnimeDbContext> getContext;
        private readonly IFtpClient ftpClient;
        private readonly Filesystem.Directory downloadDirectory;
        private readonly SemaphoreSlim ircConnectSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim ircDownloadQueueSemaphore = new SemaphoreSlim(1);
        private readonly ILogger logger;

        public IReadOnlyDictionary<string, DownloadProgress> CurrentDownloadStatus => this.downloadStatus.ToDictionary(pair => pair.Key, pair => pair.Value);

        public DownloadService(string host, int port, Func<IrcAnimeDbContext> getContext, IFtpClient ftpClient, Filesystem.Directory downloadDirectory, IModuleLogger moduleLogger)
        {
            this.logger = moduleLogger.CreateSubLogger("Download-Service");
            this.requestedDownloads = new ConcurrentDictionary<string, CreateDownloaderContext>();
            this.downloadStatus = new ConcurrentDictionary<string, DownloadProgress>();

            var ircClient = new IrcClient.Clients.IrcClient(
                (c) =>
                {
                    c.MessageReceived += (m) => this.logger.Log(LogLevel.Debug, $"Received \"{m}\"");
                    c.MessageSent += (m) => this.logger.Log(LogLevel.Debug, $"Sent \"{m}\"");

                    c.AddHandler(new IrcHandler(c));
                    c.AddHandler(new CtcpHandler(c));
                    c.AddHandler(
                        new DccHandler(c)
                        {
                            FileReceived = (c) =>
                            {
                                this.logger.Info($"Received incoming filedownload '{c.Filename}'");
                                if (this.requestedDownloads.TryGetValue(c.Filename, out var context))
                                {
                                    context.Downloader = c;
                                    context.ManualResetEvent.Set();
                                }
                            }
                        }
                    );
                }
            );

            this.ircConnection = ircClient.GetConnection(host, port);

            this.downloadDirectory = downloadDirectory;

            this.getContext = getContext;
            this.ftpClient = ftpClient;

            if (System.IO.Directory.Exists(tempFolder))
            {
                logger.Info($"Clearing irc temp folder : '{tempFolder}'");
                foreach (var file in System.IO.Directory.GetFiles(tempFolder))
                {
                    File.Delete(file);
                }
            }
        }

        public async Task CreateDownloader(DownloadRequest downloadRequest, CancellationToken cancellationToken)
        {
            this.logger.Info($"Incoming download request. Name: '{downloadRequest.FileName}' Bot: '{downloadRequest.BotName}' PackageNumber: '{downloadRequest.PackageNumber}'");
            using (var context = this.getContext())
            {
                if ((await context.Downloads.FindAsync(new[] { downloadRequest.FileName }, cancellationToken)) != null)
                {
                    this.logger.Info($"Already downloaded '{downloadRequest.FileName}'");
                    return;
                }
            }

            await this.ircConnectSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (!this.ircConnection.IsConnected)
                {
                    string name = "ad_" + Guid.NewGuid();
                    this.logger.Info("Connecting to IRC-Server");

                    await this.ircConnection.Connect();
                    await this.ircConnection.Send(IrcCommand.Nick, name);
                    await this.ircConnection.Send(IrcCommand.User, $"{name} 0 * :realname");
                    await this.ircConnection.Send(IrcCommand.Join, "#nibl");
                }
            }
            finally
            {
                this.ircConnectSemaphore.Release();
            }

            var createDownloaderContext = new CreateDownloaderContext()
            {
                Filename = downloadRequest.FileName,
            };
            var dccDownloaderTask = new Task<DccClient>(() =>
            {
                createDownloaderContext.ManualResetEvent.Wait(cancellationToken);
                return createDownloaderContext.Downloader;
            }, TaskCreationOptions.LongRunning);

            if (!await this.ircDownloadQueueSemaphore.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
            {
                this.logger.Warn("Download Lock took longer than 10 seconds to free!");
            }

            try
            {
                if (this.requestedDownloads.ContainsKey(downloadRequest.FileName))
                {
                    this.logger.Info($"Already downloading '{downloadRequest.FileName}'");
                    return;
                }
                if (this.requestedDownloads.TryAdd(downloadRequest.FileName, createDownloaderContext))
                {
                    await this.ircConnection.Send(IrcCommand.PrivMsg, $"{downloadRequest.BotName} :xdcc send #{downloadRequest.PackageNumber}");
                }
            }
            finally
            {
                this.ircDownloadQueueSemaphore.Release();
            }

            this.logger.Info($"Starting download '{downloadRequest.FileName}'");
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
                }, cancellationToken);

                context.ChangeTracker.DetectChanges();
                await context.SaveChangesAsync(cancellationToken);
            }

            this.logger.Info($"Download preparations complete. Initiate download '{downloadRequest.FileName}'");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await this.DownloadFile(downloader, cancellationToken), cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task DownloadFile(DccClient downloader, CancellationToken cancellationToken)
        {
            if (this.downloadStatus.TryAdd(downloader.Filename, new DownloadProgress() { Size = downloader.Filesize }))
            {
                var previousPercent = 0.0;
                DateTime start = default;
                downloader.Progress += (downloadedBytes, size) =>
                {
                    var percent = Math.Round((double)downloadedBytes / (double)size, 4) * 100.0;
                    if (percent % 2 == 0 && previousPercent != percent)
                    {
                        previousPercent = percent;
                        var speed = (downloadedBytes / 1024) / (DateTime.Now - start).TotalSeconds;
                        this.logger.Info($"{downloader.Filename}: {Math.Round(percent, 2)}% {string.Format("{0:n}", speed)} KB/s");
                    }
                    UpdateProgress(downloader, downloadedBytes);
                };

                this.logger.Info($"Start download '{downloader.Filename}'");
                start = DateTime.Now;
                this.logger.Info($"Download finished '{downloader.Filename}'");

                this.downloadStatus.TryRemove(downloader.Filename, out _);

                async Task<Stream> getStream(CancellationToken cancellationToken)
                    => await Task.FromResult(new FileStream(Path.Combine(this.tempFolder, downloader.Filename), FileMode.Open, FileAccess.Read));

                void onDispose()
                {
                    if (File.Exists(Path.Combine(this.tempFolder, downloader.Filename)))
                    {
                        this.logger.Info($"deleting temporary file '{downloader.Filename}'");
                        File.Delete(Path.Combine(this.tempFolder, downloader.Filename));
                    }
                }

                using var file = new Filesystem.File(
                    downloader.Filename,
                    this.downloadDirectory,
                    getStream,
                    onDispose);

                using (var stream = await file.GetStreamAsync(cancellationToken))
                {
                    await downloader.WriteToStream(stream, cancellationToken);
                }

                using (var context = this.getContext())
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                    var databaseDownloadEntry = await context.Downloads.FindAsync(downloader.Filename);
                    if (databaseDownloadEntry != null)
                    {
                        context.Downloads.Update(databaseDownloadEntry);
                        using var sha256 = SHA256.Create();
                        using var fileStream = await file.GetStreamAsync(cancellationToken);

                        databaseDownloadEntry.Sha256 = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "");
                        databaseDownloadEntry.Date = DateTime.Now;
                        databaseDownloadEntry.FtpPath = file.FullPath(Separator);

                        this.logger.Info($"Uploading '{downloader.Filename}' to FTP");
                        await this.ftpClient.UploadFileAsync(file, cancellationToken);
                        databaseDownloadEntry.DownloadStatus = DownloadStatus.Available;

                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync();
                    }
                }
                this.logger.Info($"Download completed '{downloader.Filename}'");
            }
        }

        private void UpdateProgress(DccClient downloader, ulong downloadedBytes)
        {
            if (this.downloadStatus.TryGetValue(downloader.Filename, out var downloadProgress))
            {
                downloadProgress.DownloadedBytes = downloadedBytes;
            }
        }

        private class CreateDownloaderContext
        {
            public string Filename { get; set; }

            public DccClient Downloader { get; set; }

            public ManualResetEventSlim ManualResetEvent { get; } = new ManualResetEventSlim(false);
        }
    }
}
