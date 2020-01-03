using System.Text.Json.Serialization;

namespace Cida.Client.Avalonia.Module
{
    public class AvaloniaModuleMetadata
    {
        [JsonPropertyName("assemblyFile")]
        public string AssemblyFile { get; set; } = string.Empty;
        [JsonPropertyName("entryType")]
        public string EntryType { get; set; } = string.Empty;
    }
}