using System;
using System.IO;
using System.Threading.Tasks;

namespace Cida.Api.Models.Filesystem
{
    public class File : DirectoryItem
    {
        private Stream stream;

        public File(string name, Directory directory, Stream stream)
            : base(name, directory)
        {
            this.stream = stream;
        }

        public File ReplaceContent(Stream stream)
        {
            if (this.Disposed)
            {
                throw new InvalidOperationException("This file is already disposed");
            }
            
            var file = new File(this.Name, this.Directory, stream);
            this.Dispose();
            return file;
        }

        public async Task<MemoryStream> GetStreamAsync()
        {
            if (this.Disposed)
            {
                throw new InvalidOperationException("This file is already disposed");
            }
            
            var memoryStream = new MemoryStream();

            if (this.stream.CanSeek)
            {
                this.stream.Seek(0, SeekOrigin.Begin);
            }

            await this.stream.CopyToAsync(memoryStream);
            
            if (this.stream.CanSeek)
            {
                this.stream.Seek(0, SeekOrigin.Begin);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.stream?.Dispose();
            }
        }
    }
}