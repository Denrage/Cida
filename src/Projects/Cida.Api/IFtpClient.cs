using Cida.Api.Models.Filesystem;

namespace Cida.Api;

public interface IFtpClient
{
    Task<Models.Filesystem.File> DownloadFileAsync(Models.Filesystem.File file, CancellationToken cancellationToken);
        
    Task UploadFileAsync(Models.Filesystem.File file, CancellationToken cancellationToken);
}
