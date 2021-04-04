using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Cida.Api;
using IrcClient.Clients;
using IrcClient.Commands;
using IrcClient.Connections;
using IrcClient.Handlers;
using Module.IrcAnime.Cida.Models;
using Module.IrcAnime.Cida.Models.Database;
using NLog;
using Filesystem = Cida.Api.Models.Filesystem;

namespace Module.IrcAnime.Cida.Services
{
    public class DownloadService
    {
        private const string Separator = "/";
        private readonly string tempFolder = Path.Combine(Path.GetTempPath(), "IrcDownloads");
        private readonly ILogger logger;
        private readonly Func<IrcAnimeDbContext> getContext;
        private readonly IFtpClient ftpClient;
        private readonly Filesystem.Directory downloadDirectory;
        private readonly ConcurrentDictionary<string, CancellationToken> requestedDownloads = new ConcurrentDictionary<string, CancellationToken>();
        private readonly ConcurrentDictionary<string, DownloadProgress> downloadStatus = new ConcurrentDictionary<string, DownloadProgress>();

        private RefCounted<IrcConnection> ircConnection;

        public IReadOnlyDictionary<string, DownloadProgress> CurrentDownloadStatus => this.downloadStatus.ToDictionary(pair => pair.Key, pair => pair.Value);

        public DownloadService(
            string host,
            int port,
            Func<IrcAnimeDbContext> getContext,
            IFtpClient ftpClient,
            Filesystem.Directory downloadDirectory,
            IModuleLogger moduleLogger)
        {
            this.logger = moduleLogger.CreateSubLogger("Download-Service");
            this.getContext = getContext;
            this.ftpClient = ftpClient;
            this.downloadDirectory = downloadDirectory;

            this.InitializeIrcConnection(host, port, moduleLogger.CreateSubLogger("IrcClient"));
        }

        private void InitializeIrcConnection(string host, int port, ILogger ircClientLogger)
        {
            string name = "ad_" + Guid.NewGuid();
            var ircClient = new IrcClient.Clients.IrcClient(ircConnection =>
            {
                ircConnection.MessageReceived += message => ircClientLogger.Debug($"Received message - {message}");
                ircConnection.MessageSent += message => ircClientLogger.Debug($"Sent message - {message}");

                ircConnection.AddHandler(new IrcHandler(ircConnection));
                ircConnection.AddHandler(new CtcpHandler(ircConnection));
                ircConnection.AddHandler(new DccHandler(ircConnection)
                {
                    FileReceived = connection =>
                    {
                        ircClientLogger.Info($"Received incoming filedownload '{connection.Filename}'");
                        if (this.requestedDownloads.TryGetValue(connection.Filename, out var token))
                        {
                            Task.Run(async () => await this.Download(connection, token), token);
                        }
                    }
                });
            });

            this.ircConnection = new RefCounted<IrcConnection>(
                async token =>
                {
                    var connection = ircClient.GetConnection(host, port);
                    ircClientLogger.Info($"Connecting to IRC-Server at '{host}:{port}'");
                    await connection.Connect(token);
                    await connection.Send(IrcCommand.Nick, name, token);
                    await connection.Send(IrcCommand.User, $"{name} 0 * :realname", token);
                    await connection.WaitUntilMotdReceived();
                    await connection.Send(IrcCommand.Join, "#nibl", token);

                    return connection;
                });
        }

        public async Task InitiateDownload(DownloadRequest downloadRequest, CancellationToken token)
        {
            this.logger.Info($"Incoming download request. Name: '{downloadRequest.FileName}' Bot: '{downloadRequest.BotName}' PackageNumber: '{downloadRequest.PackageNumber}'");

            if (await this.AlreadyDownloaded(downloadRequest, token))
            {
                return;
            }

            this.logger.Info($"Acquire IrcConnection for '{downloadRequest.FileName}'");
            var ircConnection = await this.ircConnection.Acquire(token);

            if (this.requestedDownloads.ContainsKey(downloadRequest.FileName))
            {
                this.logger.Info($"Already downloading '{downloadRequest.FileName}'");
                this.logger.Info($"Releasing IrcConnection for '{downloadRequest.FileName}'");
                await this.ircConnection.Release(token);
                return;
            }
            if (this.requestedDownloads.TryAdd(downloadRequest.FileName, token))
            {
                this.logger.Info("Sending DCC message to bot");
                await ircConnection.Send(IrcCommand.PrivMsg, $"{downloadRequest.BotName} :xdcc send #{downloadRequest.PackageNumber}", token);
            }
        }

        private async Task Download(DccClient downloadClient, CancellationToken token)
        {
            try
            {
                this.logger.Info($"Prepare DCC Download for '{downloadClient.Filename}'");
                if (this.downloadStatus.TryAdd(downloadClient.Filename, new DownloadProgress() { Size = downloadClient.Filesize }))
                {
                    var previousPercent = 0.0;
                    DateTime start = default;
                    downloadClient.Progress += (downloadedBytes, size) =>
                    {
                        var percent = Math.Round((double)downloadedBytes / (double)size, 4) * 100.0;
                        if (percent % 2 == 0 && previousPercent != percent)
                        {
                            previousPercent = percent;
                            var speed = (downloadedBytes / 1024) / (DateTime.Now - start).TotalSeconds;
                            this.logger.Info($"{downloadClient.Filename}: {Math.Round(percent, 2)}% {string.Format("{0:n}", speed)} KB/s");
                        }
                        UpdateProgress(downloadClient, downloadedBytes);
                    };

                    if (File.Exists(Path.Combine(this.tempFolder, downloadClient.Filename)))
                    {
                        this.logger.Info($"deleting already existing file '{downloadClient.Filename}'");
                        File.Delete(Path.Combine(this.tempFolder, downloadClient.Filename));
                    }

                    async Task<Stream> getStream(CancellationToken cancellationToken)
                        => await Task.FromResult(new FileStream(Path.Combine(this.tempFolder, downloadClient.Filename), FileMode.OpenOrCreate, FileAccess.ReadWrite));

                    void onDispose()
                    {
                        if (File.Exists(Path.Combine(this.tempFolder, downloadClient.Filename)))
                        {
                            this.logger.Info($"deleting temporary file '{downloadClient.Filename}'");
                            File.Delete(Path.Combine(this.tempFolder, downloadClient.Filename));
                        }
                    }

                    using (var context = this.getContext())
                    {
                        context.ChangeTracker.AutoDetectChangesEnabled = false;
                        await context.Downloads.AddAsync(new Download()
                        {
                            Name = downloadClient.Filename,
                            Size = downloadClient.Filesize,
                            DownloadStatus = DownloadStatus.Downloading,
                        }, token);

                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(token);
                    }

                    using var file = new Filesystem.File(
                        downloadClient.Filename,
                        this.downloadDirectory,
                        getStream,
                        onDispose);

                    this.logger.Info($"Start download '{downloadClient.Filename}'");
                    start = DateTime.Now;
                    using (var stream = await file.GetStreamAsync(token))
                    {
                        await downloadClient.WriteToStream(stream, token);
                    }

                    this.logger.Info($"Download finished '{downloadClient.Filename}' in {(DateTime.Now - start).TotalSeconds} seconds");
                    this.downloadStatus.TryRemove(downloadClient.Filename, out _);

                    this.logger.Info($"Executing post download for '{downloadClient.Filename}'");
                    await this.PostDownload(downloadClient, file, token);
                    this.logger.Info($"Download completed '{downloadClient.Filename}'");

                    this.logger.Info($"Releasing IrcConnection for '{downloadClient.Filename}'");
                    await this.ircConnection.Release(token);
                }
                else
                {
                    this.logger.Error("Couldn't add download in downloadstatus list because it exists already. This should never happen!");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured while downloading");
            }
        }

        private async Task PostDownload(DccClient downloadClient, Filesystem.File file, CancellationToken token)
        {
            using var context = this.getContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var databaseDownloadEntry = await context.Downloads.FindAsync(new[] { downloadClient.Filename }, cancellationToken: token);
            if (databaseDownloadEntry != null)
            {
                context.Downloads.Update(databaseDownloadEntry);
                using var sha256 = SHA256.Create();
                using (var fileStream = await file.GetStreamAsync(token))
                {
                    databaseDownloadEntry.Sha256 = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "");
                }

                databaseDownloadEntry.Date = DateTime.Now;
                databaseDownloadEntry.FtpPath = file.FullPath(Separator);

                this.logger.Info($"Uploading '{downloadClient.Filename}' to FTP");
                await this.ftpClient.UploadFileAsync(file, token);
                databaseDownloadEntry.DownloadStatus = DownloadStatus.Available;

                context.ChangeTracker.DetectChanges();
                await context.SaveChangesAsync(token);
            }
        }

        private async Task<bool> AlreadyDownloaded(DownloadRequest downloadRequest, CancellationToken token)
        {
            using (var context = this.getContext())
            {
                var existingDownload = (await context.Downloads.FindAsync(new[] { downloadRequest.FileName }, token));
                if (existingDownload != null)
                {
                    if (existingDownload.DownloadStatus == DownloadStatus.Available)
                    {
                        this.logger.Info($"Already downloaded '{downloadRequest.FileName}'");
                        return true;
                    }
                    else
                    {
                        context.Remove(existingDownload);
                        await context.SaveChangesAsync(token);
                        this.logger.Info($"Download for '{downloadRequest.FileName}' didn't finish successfully, redownloading ...");
                        return false;
                    }
                }
            }

            this.logger.Info($"'{downloadRequest.FileName}' is not yet downloaded!");
            return false;
        }

        private void UpdateProgress(DccClient downloader, ulong downloadedBytes)
        {
            if (this.downloadStatus.TryGetValue(downloader.Filename, out var downloadProgress))
            {
                downloadProgress.DownloadedBytes = downloadedBytes;
            }
        }
    }
}
