using System;
using System.Collections.Generic;
using System.IO;
using Filesystem = Cida.Api.Models.Filesystem;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;
using Directory = Cida.Api.Models.Filesystem.Directory;

namespace Cida.Server.Infrastructure
{
    public class FtpClient : IFtpClient
    {
        private const string Separator = "/";
        private readonly ILogger logger;
        private GlobalConfigurationManager.ExternalServerConnectionManager settings;

        public FtpClient(GlobalConfigurationService globalConfiguration, ILogger logger)
        {
            this.logger = logger;
            globalConfiguration.ConfigurationChanged +=
                () => this.settings = globalConfiguration.ConfigurationManager.Ftp;
        }

        public async Task<IEnumerable<string>> GetFilesAsync(Directory directory)
        {
            this.logger.Info("Getting files for path: {value1}", directory.FullPath(Separator));
            var request = this.CreateRequest(directory.FullPath(Separator));
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

        public async Task<Filesystem.File> GetFileAsync(Filesystem.File file)
        {
            this.logger.Info("Downloading File: {value1}", file.FullPath(Separator));
            var request = this.CreateRequest(file.FullPath(Separator));
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                var memoryStream = new MemoryStream();
                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                this.logger.Info("Downloaded File: {value1}", file.FullPath());
                return file.ReplaceContent(memoryStream);
            }

            return null;
        }

        public async Task SaveFileAsync(Filesystem.File file)
        {
            this.logger.Info("Uploading file: {value1}", file.FullPath(Separator));

            var directories = new List<Directory>();
            var currentDir = file.Directory;
            while (currentDir != null)
            {
                directories.Add(currentDir);
                currentDir = currentDir.Directory;
            }

            directories.Reverse();

            foreach (var directory in directories)
            {
                var createDirectoryRequest = this.CreateRequest(directory.FullPath((Separator)));
                createDirectoryRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                try
                {
                    using var createDirectoryResponse = await createDirectoryRequest.GetResponseAsync();
                }
                catch (WebException ex)
                {
                    if (ex.Response is FtpWebResponse makeDirectoryResponse)
                    {
                        // If File is unavailable the folder already exists
                        if (makeDirectoryResponse.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            this.logger.Error(ex);
                        }
                    }
                    else
                    {
                        this.logger.Error(ex);
                    }
                }
            }

            var request = this.CreateRequest(file.FullPath(Separator));
            request.Method = WebRequestMethods.Ftp.UploadFile;

            await using var stream = await request.GetRequestStreamAsync();
            await using var fileStream = await file.GetStreamAsync();
            await fileStream.CopyToAsync(stream);
            using var response = await request.GetResponseAsync();
            this.logger.Info("Uploaded file: {value1}", file.FullPath(Separator));
        }


        private FtpWebRequest CreateRequest(string path)
        {
            var result = (FtpWebRequest) WebRequest.Create(
                $"ftp://{this.settings.Host}:{this.settings.Port}{Separator}{path}");
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
                if (response.StatusCode != FtpStatusCode.OpeningData &&
                    response.StatusCode != FtpStatusCode.DataAlreadyOpen)
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
        Task<IEnumerable<string>> GetFilesAsync(Directory directory);

        Task<Filesystem.File> GetFileAsync(Filesystem.File file);

        Task SaveFileAsync(Filesystem.File file);
    }
}