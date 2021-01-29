using System;
using System.Collections.Generic;
using System.Text;
using Cida.Api;
using Cida.Server.Interfaces;
using NLog;

namespace Cida.Server.Module
{
    public class ModuleLoggerFactory : IModuleLoggerFactory
    {
        private readonly LogFactory factory;

        public ModuleLoggerFactory(NLog.LogFactory factory)
        {
            this.factory = factory;
        }

        public IModuleLogger Create(string name)
        {
            var logger = this.factory.GetLogger<ModuleLogger>("Module: " + name);
            logger.Initialize(new ModuleSubLoggerFactory(logger.Name, factory));

            return logger;
        }
    }
}
