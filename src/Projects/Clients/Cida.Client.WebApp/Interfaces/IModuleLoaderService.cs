using Cida.Client.WebApp.Module;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cida.Client.WebApp.Interfaces
{
    public interface IModuleLoaderService
    {
        public IAsyncEnumerable<WebModule> LoadModulesAsync();
    }
}
