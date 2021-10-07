using FS = Cida.Api.Models.Filesystem;

namespace Cida.Server.Interfaces;

public interface IModuleFtpClientFactory
{
    Cida.Api.IFtpClient Create(FS.Directory moduleDirectory);
}
