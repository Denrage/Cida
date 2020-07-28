using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Interfaces
{
    public interface IModuleFtpClientFactory
    {
        Cida.Api.IFtpClient Create(string rootPath, string moduleId);
    }
}
