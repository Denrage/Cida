using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cida.Api.Models.Filesystem;
using Cida.Server.Infrastructure;

namespace Cida.Server.Module
{
    public class ModuleFtpClient : Cida.Api.IFtpClient
    {
        private readonly Directory ModuleDirectory;
        private readonly IFtpClient ftpClient;

        public ModuleFtpClient(IFtpClient ftpClient, Directory directory)
        {
            this.ftpClient = ftpClient;
            this.ModuleDirectory = directory;
        }

        public async Task<File> DownloadFileAsync(File file)
        {
            if (file.IsInDirectory(this.ModuleDirectory))
            {
                return await this.ftpClient.GetFileAsync(file);
            }

            throw new InvalidOperationException("File to download needs to be in the module directory");
        }

        public async Task UploadFileAsync(File file)
        {
            if (file.IsInDirectory(this.ModuleDirectory))
            {
                await this.ftpClient.SaveFileAsync(file);
            }
            else
            {
                throw new InvalidOperationException("File to upload needs to be in the module directory");
            }
        }
    }
}
