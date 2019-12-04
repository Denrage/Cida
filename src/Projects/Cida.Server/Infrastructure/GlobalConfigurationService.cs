using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Cida.Server.Annotations;
using Cida.Server.Infrastructure.Database;
using Cida.Server.Module;
using Infrastructure;
using NLog;
using NLog.Fluent;

namespace Cida.Server.Infrastructure
{
    public class GlobalConfigurationService
    {
        private readonly ILogger logger;
        public event Action ConfigurationChanged;

        public GlobalConfigurationManager ConfigurationManager { get; private set; }

        // TODO: give back a copy
        public GlobalConfiguration Configuration { get; private set; }

        public GlobalConfigurationService(ILogger logger, GlobalConfiguration configuration = default)
        {
            this.logger = logger;
            this.Configuration = configuration ?? new GlobalConfiguration();
            this.ConfigurationManager = new GlobalConfigurationManager(this.Configuration);
        }

        public void Synchronize(Action<GlobalConfiguration> configurationChangeHandler)
        {
            // TODO: Use copy
            var copiedConfiguration = this.Configuration;
            configurationChangeHandler.Invoke(copiedConfiguration);
            this.logger.Info("Setting new configuration: {@value1}", copiedConfiguration);
            this.ConfigurationManager = new GlobalConfigurationManager(copiedConfiguration);
            this.ConfigurationChanged?.Invoke();
        }

        public void Update(Action<GlobalConfiguration> configurationChangeHandler, bool updateTimestamp = true)
        {
            // TODO: Use copy
            var copiedConfiguration = this.Configuration;
            configurationChangeHandler.Invoke(copiedConfiguration);
            
            if (updateTimestamp)
            {
                this.logger.Info("Updating timestamp of new configuration");
                copiedConfiguration.Timestamp = DateTime.Now;
            }

            this.logger.Info("Setting new configuration: {@value1}", copiedConfiguration);
            this.Configuration = copiedConfiguration;
            this.ConfigurationManager = new GlobalConfigurationManager(this.Configuration);
            this.ConfigurationChanged?.Invoke();
        }
    }
}