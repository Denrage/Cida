using System;
using System.Collections.Generic;
using System.Text;
using Cida.Server.Infrastructure;
using Cida.Server.Interfaces;

namespace Cida.Server.Module
{
    public class ModuleFtpClientFactory : IModuleFtpClientFactory
    {
        private readonly IFtpClient ftpClient;

        public ModuleFtpClientFactory(IFtpClient ftpClient)
        {
            this.ftpClient = ftpClient;
        }
        public Cida.Api.IFtpClient Create(string rootPath)
            => new ModuleFtpClient(this.ftpClient, rootPath);
    }
}
