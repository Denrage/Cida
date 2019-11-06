using System;

namespace Cida.Server.Module
{
    public class CidaModuleMetadata
    {
        public Guid Id { get; set; }
        
        public string AssemblyFile { get; set; } = string.Empty;

        public string EntryType { get; set; } = string.Empty;
    }
}