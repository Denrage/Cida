using Module.DatabaseConnector.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.DatabaseConnector.Settings
{
    public interface IDatabaseSettingsManager
    {
        public event Action settingsChanged;

        public ConnectionSettings LoadSettings();

        public void SaveSettings(ConnectionSettings connectionSettings);
    }
}
