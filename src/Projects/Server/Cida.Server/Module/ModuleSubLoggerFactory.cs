using System;
using System.Collections.Generic;
using System.Text;
using Cida.Api;
using Cida.Server.Interfaces;
using NLog;

namespace Cida.Server.Module
{
    public class ModuleSubLoggerFactory : IModuleSubLoggerFactory
    {
        private readonly string loggerName;
        private readonly LogFactory logFactory;

        public ModuleSubLoggerFactory(string loggerName, NLog.LogFactory logFactory)
        {
            this.loggerName = loggerName;
            this.logFactory = logFactory;
        }

        public ILogger Create(string name) => this.logFactory.GetLogger($"{this.loggerName} - {name}");
    }
}
