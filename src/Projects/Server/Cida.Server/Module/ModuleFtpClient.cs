using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cida.Server.Infrastructure;

namespace Cida.Server.Module
{
    public class ModuleFtpClient : Cida.Api.IFtpClient
    {
        private readonly string rootPath;
        private readonly IFtpClient ftpClient;

        public ModuleFtpClient(IFtpClient ftpClient, string rootPath)
        {
            this.ftpClient = ftpClient;
            this.rootPath = rootPath;
        }

        public async Task<Stream> DownloadFileAsync(params string[] path)
        {
            return new MemoryStream((await this.ftpClient.GetFileAsync(new[] { this.rootPath }.Concat(path).ToArray())).ToArray());
        }

        public async Task UploadFileAsync(Stream file, params string[] path)
        {
            if(file.CanSeek)
            {
                file.Seek(0, SeekOrigin.Begin);
            }
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            await this.ftpClient.SaveFileAsync(memoryStream.ToArray(), new[] { this.rootPath }.Concat(path).ToArray());
        }
    }
}
