using System;

namespace Cida.Server.Infrastructure
{
    public class GlobalConfigurationManager
    {
        private readonly GlobalConfiguration configuration;

        public DateTime Timestamp => this.configuration.Timestamp;

        public ExternalServerConnectionManager Ftp { get; }

        public DatabaseConnectionManager Database { get; }
        
        public GlobalConfigurationManager(GlobalConfiguration configuration)
        {
            this.configuration = configuration;
            this.Ftp = new ExternalServerConnectionManager(configuration.Ftp);
            this.Database = new DatabaseConnectionManager(configuration.Database);
        }

        public class ExternalServerConnectionManager
        {
            private readonly ExternalServerConnection configuration;
            
            public string Host => this.configuration.Host;

            public int Port => this.configuration.Port;

            public string Username => this.configuration.Username;

            public string Password => this.configuration.Password;

            public ExternalServerConnectionManager(ExternalServerConnection configuration)
            {
                this.configuration = configuration;
            }
        }

        public class DatabaseConnectionManager
        {
            private readonly DatabaseConnection configuration;

            public string DatabaseName => this.configuration.DatabaseName;

            public ExternalServerConnectionManager Connection { get; }

            public DatabaseConnectionManager(DatabaseConnection configuration )
            {
                this.configuration = configuration;
                this.Connection = new ExternalServerConnectionManager(this.configuration.Connection);
            }
        }
    }

    public class GlobalConfiguration
    {
        public DateTime Timestamp { get; set; }
        
        public ExternalServerConnection Ftp { get; set; } = new ExternalServerConnection();
        
        public DatabaseConnection Database { get; set; } = new DatabaseConnection();
    }

    public class ExternalServerConnection
    {
        public string Host { get; set; } = string.Empty;
        
        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;
    }

    public class DatabaseConnection
    {
        public string DatabaseName { get; set; } = string.Empty;
        
        public ExternalServerConnection Connection { get; set; } = new ExternalServerConnection();
    }
}