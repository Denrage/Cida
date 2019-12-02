using Cida.Server.Infrastructure.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Settings
{
    public class MockSettingsManager : IDatabaseSettingsManager
    {
        public event Action settingsChanged;

        public ConnectionSettings LoadSettings()
        {
            return new ConnectionSettings()
            {
                ServerName = "10.13.73.170",
                DatabaseName = "Cida",
                Username = "Cida",
                Password = "cida",
            };
        }
        public void SaveSettings(ConnectionSettings connectionSettings) => throw new NotImplementedException();
    }
}
