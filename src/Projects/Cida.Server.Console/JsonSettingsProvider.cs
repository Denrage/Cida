using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cida.Server.Interfaces;

namespace Cida.Server.Console
{
    public class JsonSettingsProvider : ISettingsProvider
    {
        private readonly ISettingsWriter settingsWriter;

        public JsonSettingsProvider(ISettingsWriter settingsWriter)
        {
            this.settingsWriter = settingsWriter;

            this.settingsWriter.Save(JsonSerializer.Serialize(new Dictionary<string, object>(), new JsonSerializerOptions()
            {
                WriteIndented = true,
            }));
        }

        public T Get<T>()
            where T : class
        {
            var type = typeof(T);

            if (!string.IsNullOrEmpty(type.FullName))
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(this.settingsWriter.Get());

                if (dictionary.TryGetValue(type.FullName, out var settings))
                {
                    return JsonSerializer.Deserialize<T>(settings.GetRawText());
                }
            }

            return default;
        }

        public void Save<T>(T settings)
            where T : class
        {
            var type = typeof(T);

            if (!string.IsNullOrEmpty(type.FullName))
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(this.settingsWriter.Get());

                dictionary[type.FullName] = settings;

                this.settingsWriter.Save(JsonSerializer.Serialize(dictionary));
            }
        }
    }
}