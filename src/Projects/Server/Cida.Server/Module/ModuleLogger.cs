using System;
using System.Collections.Generic;
using System.Text;
using Cida.Api;
using Cida.Server.Interfaces;
using NLog;

namespace Cida.Server.Module
{
    public class ModuleLogger : NLog.Logger, IModuleLogger
    {
        private IModuleSubLoggerFactory moduleSubLoggerFactory;

        internal void Initialize(IModuleSubLoggerFactory moduleSubLoggerFactory)
        {
            this.moduleSubLoggerFactory = moduleSubLoggerFactory;
        }

        public ILogger CreateSubLogger(string name) => this.moduleSubLoggerFactory.Create(name);
    }
}
