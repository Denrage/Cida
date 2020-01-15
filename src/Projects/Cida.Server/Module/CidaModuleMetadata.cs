using System;

namespace Cida.Server.Module
{
    // TODO: Add name
    public class CidaModuleMetadata
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public string AssemblyFile { get; set; } = string.Empty;

        public string EntryType { get; set; } = string.Empty;
    }
}