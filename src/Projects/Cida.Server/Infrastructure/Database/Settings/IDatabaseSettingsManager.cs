using Cida.Server.Infrastructure.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Server.Infrastructure.Database.Settings
{
    public interface IDatabaseSettingsManager
    {
        event Action settingsChanged;

        ConnectionSettings LoadSettings();

        void SaveSettings(ConnectionSettings connectionSettings);
    }
}
