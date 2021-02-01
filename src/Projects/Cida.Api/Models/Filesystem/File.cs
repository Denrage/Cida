using System;
using System.IO;
using System.Threading.Tasks;

namespace Cida.Api.Models.Filesystem
{
    public class File : DirectoryItem
    {
        private Func<Task<Stream>> getStream;

        public File(string name, Directory directory, Func<Task<Stream>> getStream)
            : base(name, directory)
        {
            this.getStream = getStream;
        }

        public File ReplaceContent(Func<Task<Stream>> getStream)
        {
            if (this.Disposed)
            {
                throw new InvalidOperationException("This file is already disposed");
            }
            
            var file = new File(this.Name, this.Directory, getStream);
            this.Dispose();
            return file;
        }

        public async Task<Stream> GetStreamAsync()
        {
            if (this.Disposed)
            {
                throw new InvalidOperationException("This file is already disposed");
            }

            return await this.getStream();
        }
    }
}