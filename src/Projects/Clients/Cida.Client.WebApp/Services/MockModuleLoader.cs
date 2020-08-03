using Cida.Client.WebApp.Api.BaseClasses;
using Cida.Client.WebApp.Interfaces;
using Cida.Client.WebApp.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Cida.Client.WebApp.Services
{
    public class MockModuleLoader : IModuleLoaderService
    {
        public MockModuleLoader()
        {
        }

        public async IAsyncEnumerable<WebModule> LoadModulesAsync()
        {
            foreach (var module in Enumerable.Empty<WebModule>())
            {
                yield return module;
            }
        }
    }
}
