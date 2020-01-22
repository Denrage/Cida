using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cida.Server.Interfaces;
using NLog;

namespace Cida.Server.Infrastructure
{
    public class FtpClient : IFtpClient
    {
        private readonly ILogger logger;
        private GlobalConfigurationManager.ExternalServerConnectionManager[] settings;

        public FtpClient(GlobalConfigurationService globalConfiguration, ILogger logger)
        {
            this.logger = logger;
            globalConfiguration.ConfigurationChanged +=
                () => this.settings = globalConfiguration.ConfigurationManager.Ftp;
        }

        public async Task<IEnumerable<string>> GetFilesAsync(params string[] path)
        {
            foreach (var setting in this.settings)
            {
                try
                {
                    this.logger.Info("Getting files for path: {value1}", path);
                    var request = this.CreateRequest(setting, path);
                    request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                    using var response = await request.GetResponseAsync();
                    await using var responseStream = response.GetResponseStream();

                    if (responseStream != null)
                    {
                        using var streamReader = new StreamReader(responseStream);

                        return (await streamReader.ReadToEndAsync().ConfigureAwait(false))
                            .Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                break;
            }

            return Array.Empty<string>();
        }

        public async Task<IEnumerable<byte>> GetFileAsync(params string[] path)
        {
            foreach (var setting in this.settings)
            {
                try
                {
                    this.logger.Info("Downloading File: {value1}", path);
                    var request = this.CreateRequest(setting, path);
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
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                break;
            }

            return Array.Empty<byte>();
        }

        public async Task SaveFileAsync(byte[] file, params string[] path)
        {
            this.logger.Info("Uploading file: {value1}", path);
            foreach (var setting in this.settings)
            {
                var request = this.CreateRequest(setting, path);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                await using var stream = await request.GetRequestStreamAsync();
                await stream.WriteAsync(file, 0, file.Length);
                using var response = await request.GetResponseAsync();
            }

            this.logger.Info("Uploaded file: {value1}", path);
        }

        private FtpWebRequest CreateRequest(
            GlobalConfigurationManager.ExternalServerConnectionManager connectionSettings, params string[] path)
        {
            const string separator = "/";
            var result = (FtpWebRequest) WebRequest.Create(
                $"ftp://{connectionSettings.Host}:{connectionSettings.Port}{separator}{string.Join(separator, path)}");
            result.Credentials = new NetworkCredential(connectionSettings.Username, connectionSettings.Password);
            return result;
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