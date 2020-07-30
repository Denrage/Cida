using System.Collections.Generic;

namespace Cida.Api.Models.Filesystem
{
    public class Directory : DirectoryItem
    {
        public IReadOnlyList<DirectoryItem> Files => this.InternalFiles; 

        internal List<DirectoryItem> InternalFiles { get; } = new List<DirectoryItem>();
        
        public Directory(string name, Directory directory)
            : base(name, directory)
        {
        }
    }
}