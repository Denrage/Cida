using Cida.Api.Models.Filesystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Api
{
    public interface IFtpClient
    {
        Task<File> DownloadFileAsync(File file);
        
        Task UploadFileAsync(File file);
    }
}
