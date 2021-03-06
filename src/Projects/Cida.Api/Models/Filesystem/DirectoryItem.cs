using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cida.Api.Models.Filesystem
{
    public abstract class DirectoryItem : IDisposable
    {
        public const string Separator = "/";
        protected bool Disposed { get; private set; } = false;
        public string Name { get; }

        public Directory Directory { get; private set; }

        public string FullPath() => GetFullPath(this, Separator);

        public string FullPath(string separator) => GetFullPath(this, separator);

        public DirectoryItem(string name, Directory directory)
        {
            this.Name = name ??
                        throw new ArgumentNullException(nameof(name));

            this.Directory = directory;
            this.Directory?.InternalFiles.Add(this);
        }

        public void Move(Directory directory)
        {
            if (this.Disposed)
            {
                throw new InvalidOperationException("This file is already disposed");
            }

            this.Directory?.InternalFiles.Remove(this);
            this.Directory = directory;
            this.Directory?.InternalFiles.Add(this);
        }

        public void Dispose()
        {
            this.Directory?.InternalFiles.Remove(this);
            this.Directory = null;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DirectoryItem()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Disposed = true;
        }

        private static string GetFullPath(DirectoryItem item, string separator)
        {
            return item.Directory == null ? item.Name : $"{GetFullPath(item.Directory, separator)}{separator}{item.Name}";
        }

        public bool IsInDirectory(Directory directory)
        {
            var currentDirectory = this.Directory;
            while (currentDirectory != null)
            {
                if (currentDirectory == directory)
                {
                    return true;
                }

                currentDirectory = currentDirectory.Directory;
            }

            return false;
        }
    }
}