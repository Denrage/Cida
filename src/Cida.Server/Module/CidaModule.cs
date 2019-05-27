using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Newtonsoft.Json;

namespace Cida.Server.Module
{
    public class CidaModule : IDisposable
    {
        private const string PackagesInfo = "PackagesInfo.json";
        private readonly IDictionary<string, Stream> moduleFiles;
        private readonly CidaModuleLoadContext loadContext;

        public Assembly Assembly { get; }

        public CidaModuleMetadata Metadata { get; }
        
        public CidaModule(
            CidaModuleMetadata metadata, 
            IDictionary<string, Stream> moduleFiles)
        {
            this.Metadata = metadata;
            this.moduleFiles = moduleFiles;
            this.loadContext = this.InitializeLoadContext();
            
            if (!this.moduleFiles.TryGetValue(this.Metadata.AssemblyFile, out var assembly))
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"Assembly '{this.Metadata.AssemblyFile}' not found!");
            }
            
            this.Assembly = this.loadContext.LoadFromStream(assembly);
        }

        private CidaModuleLoadContext InitializeLoadContext()
        {
            var loadContext = new CidaModuleLoadContext();
            loadContext.Resolving += (context, name) =>
            {
                if (!this.moduleFiles.TryGetValue($"{name.Name}.dll", out var resolvedDependency))
                {
                    // TODO: Replace this with custom exception
                    throw new InvalidOperationException($"Assembly '{name.Name}.dll' not found.");
                }

                return context.LoadFromStream(resolvedDependency);
            };

            return loadContext;
        }

        public void Load()
        {
            var moduleType = this.Assembly.GetType(this.Metadata.EntryType);
            var instance = (Api.IModule)Activator.CreateInstance(moduleType);
            instance.Load();
        }

        ~CidaModule()
        {
            this.Dispose();
        }

        public static CidaModule Extract(string path)
        {
            var fileStreams = ExtractFiles(path);

            var parsedMetadata = ParseMetadata(fileStreams);

            return new CidaModule(parsedMetadata, fileStreams);
        }

        private static CidaModuleMetadata ParseMetadata(IDictionary<string, Stream> fileStreams)
        {
            if (!fileStreams.TryGetValue(PackagesInfo, out var metadata))
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"No '{PackagesInfo}' found in root dir of module!");
            }

            CidaModuleMetadata? parsedMetadata = null;
            using (var metadataStream = new StreamReader(metadata))
            {
                parsedMetadata = JsonConvert.DeserializeObject<CidaModuleMetadata>(metadataStream.ReadToEnd());
            }

            return parsedMetadata;
        }

        private static IDictionary<string, Stream> ExtractFiles(string path)
        {
            IDictionary<string, Stream> fileStreams = new Dictionary<string, Stream>();
            using (var archive = ZipFile.OpenRead(path))
            {
                foreach (var entry in archive.Entries)
                {
                    var entryFileStream = new MemoryStream();

                    using (var entryStream = entry.Open())
                    {
                        entryStream.CopyTo(entryFileStream);
                    }

                    entryFileStream.Seek(0, SeekOrigin.Begin);

                    fileStreams.Add(entry.FullName, entryFileStream);
                }
            }

            return fileStreams;
        }

        public void Dispose()
        {
            this.loadContext.Unload();
            foreach (var file in this.moduleFiles.Values)
            {
                file.Close();
            }
            
            GC.SuppressFinalize(this);
        }
    }
}