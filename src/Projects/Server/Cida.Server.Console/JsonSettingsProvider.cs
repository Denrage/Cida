using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cida.Server.Interfaces;

namespace Cida.Server.Console
{
    public class JsonSettingsProvider : ISettingsProvider
    {
        private readonly JsonSerializerOptions serializationOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        private readonly ISettingsWriter settingsWriter;

        public JsonSettingsProvider(ISettingsWriter settingsWriter)
        {
            this.settingsWriter = settingsWriter;

            if (string.IsNullOrEmpty(settingsWriter.Get()))
            {
                this.settingsWriter.Save(JsonSerializer.Serialize(new Dictionary<string, object>(), this.serializationOptions));
            }
        }

        public T Get<T>()
            where T : class, new()
        {
            var type = typeof(T);

            if (!string.IsNullOrEmpty(type.FullName))
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(this.settingsWriter.Get());

                if (dictionary.TryGetValue(type.FullName, out var settings))
                {
                    return JsonSerializer.Deserialize<T>(settings.GetRawText());
                }

                var result = new T();
                this.Save(result);

                return result;
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

                this.settingsWriter.Save(JsonSerializer.Serialize(dictionary, this.serializationOptions));
            }
        }
    }
}