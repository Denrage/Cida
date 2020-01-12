using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Episode
{
    internal class QualityEnumConverter : JsonConverter<Quality>
    {
        public override Quality Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();
            if (Enum.TryParse<Quality>(text, out var result))
            {
                return result;
            }

            return Quality.Adaptive;
        }

        public override void Write(Utf8JsonWriter writer, Quality value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Enum.GetName(typeof(Quality), value));
        }
    }
}