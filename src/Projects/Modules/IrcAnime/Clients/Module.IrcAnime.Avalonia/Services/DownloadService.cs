using Cida.Client.Avalonia.Api;
using Ircanime;
using Module.IrcAnime.Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.Services
{
    //TODO: Interface this
    public class DownloadService
    {
        private SemaphoreSlim downloadListSemaphore = new SemaphoreSlim(1);
        private List<string> currentDownloads = new List<string>();

        public event Action<DownloadContext, long> OnBytesDownloaded;

        public event Action<DownloadContext> OnDownloadFinished;

        public class DownloadSettings : ICloneable
        {
            public string DownloadFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cida", "Avalonia", "IrcDownloads");

            public object Clone() => this.MemberwiseClone();
        }

        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly IModuleSettingsService settingsService;

        public DownloadService(IrcAnimeService.IrcAnimeServiceClient client, IModuleSettingsService settingsService)
        {
            this.client = client;
            this.settingsService = settingsService;
        }

        private async Task<string> GetDownloadFolderAsync() => (await this.settingsService.Get<DownloadSettings>()).DownloadFolder;

        public async Task Download(DownloadContext context, CancellationToken token)
        {
            var filename = context.Pack.Name;
            await this.downloadListSemaphore.WaitAsync();
            try
            {
                if (this.currentDownloads.Contains(filename))
                {
                    return;
                }

                this.currentDownloads.Add(filename);
            }
            finally
            {
                this.downloadListSemaphore.Release();
            }

            Directory.CreateDirectory((await this.GetDownloadFolderAsync()));

            var information = await this.client.FileTransferInformationAsync(new FileTransferInformationRequest() { FileName = filename }, cancellationToken: token);
            var chunkSize = information.ChunkSize;
            var size = information.Size;
            var sha256 = information.Sha256;

            using (var filestream = File.Open(Path.Combine(await this.GetDownloadFolderAsync(), filename), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                filestream.Seek(0, SeekOrigin.End);
                using (var chunkStream = this.client.File(new FileRequest()
                {
                    FileName = filename,
                    Position = (ulong)filestream.Position
                }, cancellationToken: token))
                {
                    while (await chunkStream.ResponseStream.MoveNext(token))
                    {
                        token.ThrowIfCancellationRequested();
                        var buffer = new byte[chunkSize];
                        chunkStream.ResponseStream.Current.Chunk.CopyTo(buffer, 0);
                        await filestream.WriteAsync(buffer, 0, (int)chunkStream.ResponseStream.Current.Length, token);
                        filestream.Seek((long)chunkStream.ResponseStream.Current.Position, SeekOrigin.Begin);
#pragma warning disable CS4014 // That's exactly what we want
                        Task.Run(() => this.OnBytesDownloaded?.Invoke(context, filestream.Position));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }

                using var hash = SHA256.Create();
                filestream.Seek(0, SeekOrigin.Begin);
                var hashString = BitConverter.ToString(hash.ComputeHash(filestream)).Replace("-", "");

                if (hashString != sha256)
                {
                    // TODO: DO SOMETHING HERE
                }

                await Task.Run(() => this.OnDownloadFinished?.Invoke(context));
            }
        }
    }
}
