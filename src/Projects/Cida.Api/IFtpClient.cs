using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Api
{
    public interface IFtpClient
    {
        Task<Cida.Api.Models.Filesystem.File> DownloadFileAsync(Cida.Api.Models.Filesystem.File file);
        
        Task UploadFileAsync(Cida.Api.Models.Filesystem.File file);
    }
}
