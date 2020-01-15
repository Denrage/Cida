using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Module.Crunchyroll.Libs.Models.Search
{
    internal class TypeEnumConverter : JsonConverter<TypeEnum>
    {
        public override TypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();
            if (Enum.TryParse<TypeEnum>(text, out var result))
            {
                return result;
            }

            return TypeEnum.Unknown;
        }

        public override void Write(Utf8JsonWriter writer, TypeEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Enum.GetName(typeof(TypeEnum), value));
        }
    }
}