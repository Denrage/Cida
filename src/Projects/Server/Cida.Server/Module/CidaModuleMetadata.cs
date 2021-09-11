using System;
using System.Collections.Generic;

namespace Cida.Server.Module
{
    public class CidaModuleMetadata
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public Guid Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("assemblyFile")]
        public string AssemblyFile { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("entryType")]
        public string EntryType { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("clients")]
        public Dictionary<Guid, string> Clients { get; set; } = new Dictionary<Guid, string>();
    }
}