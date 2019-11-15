using System;
using System.Linq;
using Cida.Server.Module;

namespace Cida.Server.Infrastructure
{
    public class GlobalConfigurationService
    {
        private readonly IModulePublisher modulePublisher;

        public event Action ConfigurationChanged;
        
        public GlobalConfiguration Configuration { get; private set; } 
        
        public GlobalConfigurationService(IModulePublisher modulePublisher, GlobalConfiguration configuration = default)
        {
            this.Configuration = configuration ?? new GlobalConfiguration();
            this.modulePublisher = modulePublisher;

            this.modulePublisher.ModulesUpdated += () =>
            {
                this.Configuration.Modules = this.modulePublisher.Modules.ToArray();
            };
        }

        public class GlobalConfiguration
        {
            [NonSerialized]
            private DateTime timestamp = DateTime.Now;
            
            [NonSerialized]
            private Guid[] modules;

            [NonSerialized]
            private ExternalServerConnection ftp = new ExternalServerConnection();
            public event Action ConfigurationChanged;

            public DateTime Timestamp
            {
                get => this.timestamp;
                private set
                {
                    this.timestamp = value;
                    this.ConfigurationChanged?.Invoke();    
                }
            }

            public Guid[] Modules
            {
                get => this.modules;
                set
                {
                    this.modules = value;
                    this.RefreshTimeStamp();
                }
            }

            public ExternalServerConnection Ftp
            {
                get => this.ftp;
                set
                {
                    this.ftp = value;
                    this.RefreshTimeStamp();
                }
            }

            private void RefreshTimeStamp() 
                => this.Timestamp = DateTime.Now;

            public class ExternalServerConnection
            {
                // The properties are readonly so a new instance of this class is needed to be set in the configuration
                public string Host { get; }
                
                public int Port { get; }
                
                public string Username { get; }
                
                public string Password { get; }

                public ExternalServerConnection()
                    : this(string.Empty, 0, string.Empty, string.Empty)
                {
                        
                }

                public ExternalServerConnection(string host, int port, string username, string password)
                {
                    this.Host = host;
                    this.Port = port;
                    this.Username = username;
                    this.Password = password;
                }
            }
        }
    }
}