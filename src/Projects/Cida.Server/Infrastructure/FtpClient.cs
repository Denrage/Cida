using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cida.Server.Interfaces;

namespace Cida.Server.Infrastructure
{
    public class FtpClient : IFtpClient
    {
        private readonly FtpSettings settings;
        public FtpClient(ISettingsProvider settings)
        {
            this.settings = settings.Get<FtpSettings>();
        }

        public async Task<IEnumerable<string>> GetFilesAsync(params string[] path)
        {
            var request = this.CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                using var streamReader = new StreamReader(responseStream);

                return (await streamReader.ReadToEndAsync().ConfigureAwait(false))
                    .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            return Array.Empty<string>();
        }

        public async Task<IEnumerable<byte>> GetFileAsync(params string[] path)
        {
            var request = this.CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                await using var memoryStream = new MemoryStream();
                responseStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }

            return Array.Empty<byte>();
        }

        public async Task SaveFileAsync(byte[] file, params string[] path)
        {
            var request = this.CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            await using var stream = await request.GetRequestStreamAsync();
            await stream.WriteAsync(file, 0 , file.Length);
        }

        private FtpWebRequest CreateRequest(params string[] path)
        {
            const string separator = "/";
            var result = (FtpWebRequest)WebRequest.Create($"ftp://{this.settings.Host}:{this.settings.Port}{separator}{string.Join(separator, path)}");
            result.Credentials = new NetworkCredential(this.settings.Username, this.settings.Password);
            return result;
        }

        public class FtpSettings
        {
            public string Host { get; set; }

            public int Port { get; set; } = 21;

            public string Username { get; set; }

            public string Password { get; set; }
        }
    }

    // TODO: Move out
    public interface IFtpClient
    {
        Task<IEnumerable<string>> GetFilesAsync(params string[] path);

        Task<IEnumerable<byte>> GetFileAsync(params string[] path);

        Task SaveFileAsync(byte[] file, params string[] path);
    }

}
