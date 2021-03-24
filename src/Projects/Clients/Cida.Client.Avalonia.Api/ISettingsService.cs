using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Client.Avalonia.Api
{
    public interface IModuleSettingsService
    {
        Task Save<T>(T settings)
            where T : class, ICloneable;

        Task<T> Get<T>()
            where T : class, ICloneable, new();
    }
}
