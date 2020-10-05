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
    class DownloadService
    {
        private const string DownloadFolder = @"D:\temp";
        private readonly IrcAnimeService.IrcAnimeServiceClient client;

        public DownloadService(IrcAnimeService.IrcAnimeServiceClient client)
        {
            this.client = client;
        }

        public async Task Download(string filename, CancellationToken token)
        {
            var information = await this.client.FileTransferInformationAsync(new FileTransferInformationRequest() { FileName = filename });
            var chunkSize = information.ChunkSize;
            var size = information.Size;
            var sha256 = information.Sha256;

            using (var filestream = File.Open(Path.Combine(DownloadFolder, filename), FileMode.Append, FileAccess.Write))
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
