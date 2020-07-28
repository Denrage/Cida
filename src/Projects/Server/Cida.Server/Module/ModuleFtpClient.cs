using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cida.Api.Models.Filesystem;
using Cida.Server.Infrastructure;

namespace Cida.Server.Module
{
    public class ModuleFtpClient : Cida.Api.IFtpClient
    {
        private readonly Cida.Api.Models.Filesystem.Directory ModuleDirectory;
        private readonly IFtpClient ftpClient;

        public ModuleFtpClient(IFtpClient ftpClient, Cida.Api.Models.Filesystem.Directory directory)
        {
            this.ftpClient = ftpClient;
            this.ModuleDirectory = directory;
        }

        public async Task<Cida.Api.Models.Filesystem.File> DownloadFileAsync(Cida.Api.Models.Filesystem.File file)
        {
            file.Move(this.ModuleDirectory);
            return await this.ftpClient.GetFileAsync(file);
        }

        public async Task UploadFileAsync(Cida.Api.Models.Filesystem.File file)
        {
            await this.ftpClient.SaveFileAsync(file);
        }
    }
}
