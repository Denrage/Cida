using System;
using System.Collections.Generic;
using System.Text;

namespace Cida.Client.Avalonia.Api
{
    public interface ISettingsFactory
    {
        IModuleSettingsService Get(string moduleName);
    }
}
