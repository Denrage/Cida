using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Api
{
    public interface IFtpClient
    {
        Task<Stream> DownloadFileAsync(params string[] file);
        Task UploadFileAsync(Stream file, params string[] path);
    }
}
