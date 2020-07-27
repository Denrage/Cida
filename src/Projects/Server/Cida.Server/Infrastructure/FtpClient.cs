using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NLog;

namespace Cida.Server.Infrastructure
{
    public class FtpClient : IFtpClient
    {
        private readonly ILogger logger;
        private GlobalConfigurationManager.ExternalServerConnectionManager settings;

        public FtpClient(GlobalConfigurationService globalConfiguration, ILogger logger)
        {
            this.logger = logger;
            globalConfiguration.ConfigurationChanged +=
                () => this.settings = globalConfiguration.ConfigurationManager.Ftp;
        }

        public async Task<IEnumerable<string>> GetFilesAsync(params string[] path)
        {
            this.logger.Info("Getting files for path: {value1}", path);
            var request = this.CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                using var streamReader = new StreamReader(responseStream);

                return (await streamReader.ReadToEndAsync().ConfigureAwait(false))
                    .Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            }

            return Array.Empty<string>();
        }

        public async Task<IEnumerable<byte>> GetFileAsync(params string[] path)
        {
            this.logger.Info("Downloading File: {value1}", path);
            var request = this.CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                await using var memoryStream = new MemoryStream();
                responseStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                this.logger.Info("Downloaded File: {value1}", path);
                return memoryStream.ToArray();
            }

            return Array.Empty<byte>();
        }

        public async Task SaveFileAsync(byte[] file, params string[] path)
        {
            this.logger.Info("Uploading file: {value1}", path);

            var request = this.CreateRequest(path);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            await using var stream = await request.GetRequestStreamAsync();
            await stream.WriteAsync(file, 0, file.Length);
            using var response = await request.GetResponseAsync();
            this.logger.Info("Uploaded file: {value1}", path);
        }

        private FtpWebRequest CreateRequest(params string[] path)
        {
            const string separator = "/";
            var result = (FtpWebRequest) WebRequest.Create(
                $"ftp://{this.settings.Host}:{this.settings.Port}{separator}{string.Join(separator, path)}");
            result.Credentials = new NetworkCredential(this.settings.Username, this.settings.Password);
            return result;
        }

        public bool ValidateConfiguration(ExternalServerConnection ftpConnection)
        {
            if (string.IsNullOrEmpty(ftpConnection.Host) || string.IsNullOrEmpty(ftpConnection.Username))
            {
                return false;
            }

            return true;
        }

        public bool TryConnect(ExternalServerConnection ftpConnection, out Exception occuredException)
        {
            try
            {
                var result = (FtpWebRequest) WebRequest.Create($"ftp://{ftpConnection.Host}:{ftpConnection.Port}");
                result.Credentials = new NetworkCredential(ftpConnection.Username, ftpConnection.Password);
                result.Method = WebRequestMethods.Ftp.ListDirectory;
                using var response = (FtpWebResponse) result.GetResponse();

                // Maybe response will not be disposed correctly?
                if (response.StatusCode != FtpStatusCode.OpeningData && response.StatusCode != FtpStatusCode.DataAlreadyOpen)
                {
                    occuredException =
                        new InvalidOperationException($"Unexpected StatusCode '{response.StatusCode}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                occuredException = ex;
                return false;
            }

            occuredException = null;
            return true;
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