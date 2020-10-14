using Cida.Client.Avalonia.Api;
using Ircanime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.Services
{
    //TODO: Interface this
    public class DownloadService
    {
        public class DownloadSettings
        {
            public string DownloadFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cida", "Avalonia", "IrcDownloads");
        }

        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly IModuleSettingsService settingsService;

        public DownloadService(IrcAnimeService.IrcAnimeServiceClient client, IModuleSettingsService settingsService)
        {
            this.client = client;
            this.settingsService = settingsService;
        }

        private async Task<string> GetDownloadFolderAsync() => (await this.settingsService.Get<DownloadSettings>()).DownloadFolder;

        public async Task Download(string filename, CancellationToken token)
        {
            Directory.CreateDirectory((await this.GetDownloadFolderAsync()));

            var information = await this.client.FileTransferInformationAsync(new FileTransferInformationRequest() { FileName = filename });
            var chunkSize = information.ChunkSize;
            var size = information.Size;
            var sha256 = information.Sha256;

            using (var filestream = File.Open(Path.Combine(await this.GetDownloadFolderAsync(), filename), FileMode.Append, FileAccess.Write))
            {
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
                    }
                }
            }

        }
    }
}
