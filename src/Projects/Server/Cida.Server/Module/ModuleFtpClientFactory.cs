using FS = Cida.Api.Models.Filesystem;
using Cida.Server.Infrastructure;
using Cida.Server.Interfaces;

namespace Cida.Server.Module;

public class ModuleFtpClientFactory : IModuleFtpClientFactory
{
    private readonly IFtpClient ftpClient;

    public ModuleFtpClientFactory(IFtpClient ftpClient)
    {
        this.ftpClient = ftpClient;
    }

    public Cida.Api.IFtpClient Create(FS.Directory moduleDirectory)
    {
        return new ModuleFtpClient(this.ftpClient, moduleDirectory);
    }
}
