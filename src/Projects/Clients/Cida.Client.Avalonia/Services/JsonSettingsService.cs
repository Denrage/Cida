using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cida.Client.Avalonia.Services
{
    public class JsonSettingsService : ISettingsService
    {
        private readonly string settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cida", "Avalonia", "settings.json");
        private Settings cache;


        public JsonSettingsService()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.settingsFile));
        }

        public async Task<T> Get<T>(string moduleName)
            where T : class, new()
        {
            await this.EnsureSettingsAsync();

            if (this.cache.Modules.TryGetValue(moduleName, out var module))
            {
                if (module.Items.TryGetValue(typeof(T).FullName, out var result))
                {
                    if (result is JsonElement jsonElement)
                    {
                        return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                    }
                }
            }


            module = new Module();
            this.cache.Modules.Add(moduleName, module);
            module.ModuleName = moduleName;
            var defaultItem = new T();
            module.Items.Add(typeof(T).FullName, defaultItem);
            await this.SaveSettings(JsonSerializer.Serialize<Settings>(this.cache));
            return defaultItem;

        }

        public async Task Save<T>(string moduleName, T item)
            where T : class
        {
            await this.EnsureSettingsAsync();
            if (this.cache.Modules.TryGetValue(moduleName, out var result))
            {
                result.Items[typeof(T).FullName] = item;
            }
            else
            {
                var module = new Module();
                this.cache.Modules.Add(moduleName, module);
                module.Items = new Dictionary<string, object>();
                module.Items[typeof(T).FullName] = item;
            }
            await this.SaveSettings(JsonSerializer.Serialize<Settings>(this.cache));
        }

        private async Task SaveSettings(string json)
        {
            await File.WriteAllTextAsync(this.settingsFile, json);
        }

        private async Task EnsureSettingsAsync()
        {
            if (!File.Exists(settingsFile))
            {
                this.cache = new Settings();
                await this.SaveSettings(JsonSerializer.Serialize<Settings>(this.cache));
            }
            else
            {
                this.cache = JsonSerializer.Deserialize<Settings>(await File.ReadAllTextAsync(this.settingsFile));
            }
        }


        public class Module
        {
            public string ModuleName { get; set; }

            public Dictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
        }

        public class Settings
        {
            public Dictionary<string, Module> Modules { get; set; } = new Dictionary<string, Module>();
        }
    }
}
