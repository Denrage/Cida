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

        public Cida.Api.IFtpClient Create(string rootPath, string moduleId)
        {
            var rootDirectory = new Cida.Api.Models.Filesystem.Directory(rootPath, null);
            var moduleDirectory = new Cida.Api.Models.Filesystem.Directory(moduleId, rootDirectory);
            return new ModuleFtpClient(this.ftpClient, moduleDirectory);
        }
    }
}
