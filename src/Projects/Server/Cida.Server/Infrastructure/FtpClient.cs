using Filesystem = Cida.Api.Models.Filesystem;
using NLog;
using Directory = Cida.Api.Models.Filesystem.Directory;

namespace Cida.Server.Infrastructure;

public class FtpClient : IFtpClient
{
    private const string Separator = "/";
    private readonly ILogger logger;
    private readonly string tempFolder = Path.Combine(Path.GetTempPath(), "FtpDownloads");
    private FluentFTP.FtpClient ftpClient;
    private GlobalConfigurationManager.ExternalServerConnectionManager settings = null!;

    public FtpClient(GlobalConfigurationService globalConfiguration, ILogger logger)
    {
        this.logger = logger;

        globalConfiguration.ConfigurationChanged += () =>
        {
            this.settings = globalConfiguration.ConfigurationManager.Ftp;

            this.ftpClient = new FluentFTP.FtpClient(this.settings.Host, this.settings.Port, this.settings.Username, this.settings.Password);
            this.ftpClient.OnLogEvent += (traceLevel, message) =>
            {
                var logLevel = LogLevel.Info;
                switch (traceLevel)
                {
                    case FluentFTP.FtpTraceLevel.Verbose:
                        logLevel = LogLevel.Debug;
                        break;
                    case FluentFTP.FtpTraceLevel.Info:
                        logLevel = LogLevel.Info;
                        break;
                    case FluentFTP.FtpTraceLevel.Warn:
                        logLevel = LogLevel.Warn;
                        break;
                    case FluentFTP.FtpTraceLevel.Error:
                        logLevel = LogLevel.Error;
                        break;
                    default:
                        logLevel = LogLevel.Info;
                        break;
                }

                this.logger.Log(logLevel, message);
            };
        };

        if (System.IO.Directory.Exists(tempFolder))
        {
            logger.Info($"Clearing ftp temp folder : '{tempFolder}'");
            foreach (var file in System.IO.Directory.GetFiles(tempFolder))
            {
                File.Delete(file);
            }
        }
    }

    private async Task<bool> EnsureConnect(CancellationToken cancellationToken)
    {
        return await this.ftpClient.AutoConnectAsync(cancellationToken) != null;
    }

    public async Task<IEnumerable<string>> GetFilesAsync(Directory directory, CancellationToken cancellationToken)
    {
        this.logger.Info("Getting files for path: {value1}", directory.FullPath(Separator));
        if (!await this.EnsureConnect(cancellationToken))
        {
            this.logger.Warn("Could not ensure connection!");
            return Enumerable.Empty<string>();
        }
        var items = await this.ftpClient.GetListingAsync(directory.FullPath(Separator), cancellationToken);

        return items.Select(x => x.Name);
    }

    public async Task<Filesystem.File> GetFileAsync(Filesystem.File file, CancellationToken cancellationToken)
    {
        this.logger.Info("Downloading File: {value1}", file.FullPath(Separator));
        if (!await this.EnsureConnect(cancellationToken))
        {
            this.logger.Warn("Could not ensure connection!");
            return Filesystem.File.EmptyFile;
        }

        System.IO.Directory.CreateDirectory(this.tempFolder ?? throw new InvalidOperationException());
        var downloadResult = await this.ftpClient.DownloadFileAsync(
            Path.Combine(this.tempFolder, file.Name),
            file.FullPath(Separator),
            token: cancellationToken);

        if (downloadResult == FluentFTP.FtpStatus.Failed)
        {
            this.logger.Warn("FTP download failed!");
            return Filesystem.File.EmptyFile;
        }

        async Task<Stream> getStream(CancellationToken cancellationToken)
            => await Task.FromResult(new FileStream(Path.Combine(this.tempFolder, file.Name), FileMode.OpenOrCreate));

        void onDispose()
        {
            if (File.Exists(Path.Combine(this.tempFolder, file.Name)))
            {
                File.Delete(Path.Combine(this.tempFolder, file.Name));
            }
        }

        return file.ReplaceContent(getStream, onDispose);
    }

    public async Task SaveFileAsync(Filesystem.File file, CancellationToken cancellationToken)
    {
        try
        {
            this.logger.Info("Uploading file: {value1}", file.FullPath(Separator));
            if (!await this.EnsureConnect(cancellationToken))
            {
                this.logger.Warn("Could not ensure connection!");
            }

            string fileGuid = Guid.NewGuid().ToString();

            async Task<Stream> getStream(CancellationToken cancellationToken)
                => await Task.FromResult(new FileStream(Path.Combine(this.tempFolder, fileGuid), FileMode.OpenOrCreate));

            void onDispose()
            {
                if (File.Exists(Path.Combine(this.tempFolder, fileGuid)))
                {
                    File.Delete(Path.Combine(this.tempFolder, fileGuid));
                }
            }

            var tempFile = new Filesystem.File(file.Name, file.Directory, getStream, onDispose);
            await file.CopyToAsync(tempFile, cancellationToken);

            var uploadResult = await this.ftpClient.UploadFileAsync(Path.Combine(this.tempFolder, fileGuid), file.FullPath(Separator));

            if (uploadResult == FluentFTP.FtpStatus.Failed)
            {
                this.logger.Warn("Upload failed!");
                return;
            }

            this.logger.Info("Uploaded file: {value1}", file.FullPath(Separator));
        }
        catch (Exception ex)
        {
            this.logger.Error(ex, "Error occured while uploading file");
        }
    }

    public bool ValidateConfiguration(ExternalServerConnection ftpConnection)
    {
        if (string.IsNullOrEmpty(ftpConnection.Host) || string.IsNullOrEmpty(ftpConnection.Username))
        {
            return false;
        }

        return true;
    }

    public bool TryConnect(ExternalServerConnection ftpConnection, out Exception? occuredException)
    {
        try
        {
            var verifyClient = new FluentFTP.FtpClient(ftpConnection.Host, ftpConnection.Port, ftpConnection.Username, ftpConnection.Password);
            occuredException = null;
            return verifyClient.AutoConnect() != null;
        }
        catch (Exception ex)
        {
            occuredException = ex;
            return false;
        }
    }
}

// TODO: Move out
public interface IFtpClient
{
    Task<IEnumerable<string>> GetFilesAsync(Directory directory, CancellationToken cancellationToken);

    Task<Filesystem.File> GetFileAsync(Filesystem.File file, CancellationToken cancellationToken);

    Task SaveFileAsync(Filesystem.File file, CancellationToken cancellationToken);
}
