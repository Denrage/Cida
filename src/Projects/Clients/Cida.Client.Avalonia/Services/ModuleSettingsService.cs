using Cida.Client.Avalonia.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Client.Avalonia.Services
{
    public class ModuleSettingsService : IModuleSettingsService
    {
        private readonly ISettingsService settingsService;
        private Dictionary<Type, object> settingsCache = new Dictionary<Type, object>();
        private readonly string moduleName;


        public ModuleSettingsService(string moduleName, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.moduleName = moduleName;
        }

        public async Task<T> Get<T>() 
            where T : class, new()
        {
            var type = typeof(T);
            if (this.settingsCache.TryGetValue(type, out var result))
            {
                return (T)result;
            }

            result = await this.settingsService.Get<T>(this.moduleName);

            this.settingsCache[type] = result;
            return (T)result;
        }

        public async Task Save<T>(T settings) 
            where T : class
        {
            var type = typeof(T);
            this.settingsCache[type] = settings;
            await this.settingsService.Save<T>(this.moduleName, settings);
        }
    }
}
