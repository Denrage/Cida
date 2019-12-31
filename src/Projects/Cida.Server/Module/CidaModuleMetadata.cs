using System;
using System.Collections.Generic;

namespace Cida.Server.Module
{
    public class CidaModuleMetadata
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public string AssemblyFile { get; set; } = string.Empty;

        public string EntryType { get; set; } = string.Empty;

        public Dictionary<Guid, string> Clients { get; set; } = new Dictionary<Guid, string>();
    }
}