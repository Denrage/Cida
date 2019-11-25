using Cida.Server.Infrastructure.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Settings
{
    public interface IDatabaseSettingsManager
    {
        public event Action settingsChanged;

        public ConnectionSettings LoadSettings();

        public void SaveSettings(ConnectionSettings connectionSettings);
    }
}
