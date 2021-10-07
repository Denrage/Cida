using System.Text.Json.Serialization;

namespace Cida.Server.Infrastructure.Database.ProviderLoader;

public class DatabaseProviderMetadata
{
    [JsonPropertyName("assemblyFile")]
    public string AssemblyFile { get; set; } = string.Empty;
    [JsonPropertyName("entryType")]
    public string EntryType { get; set; } = string.Empty;
}
