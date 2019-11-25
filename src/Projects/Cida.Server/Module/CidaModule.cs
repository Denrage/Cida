using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cida.Api;
using Cida.Server.Infrastructure;
using Newtonsoft.Json;

namespace Cida.Server.Module
{
    public class CidaModule : IDisposable
    {
        private const string PackagesInfo = "PackagesInfo.json";
        private readonly IDictionary<string, Stream> moduleFiles;
        private readonly CidaModuleLoadContext loadContext;
        private readonly Type entryType;

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

            this.entryType = this.Assembly.GetType(this.Metadata.EntryType);
            if (this.entryType is null)
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"Entry type '{this.Metadata.EntryType}' not found in assembly");
            }
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

        public IModule Load()
        {
            var instance = (Cida.Api.IModule) Activator.CreateInstance(this.entryType);
            instance.Load();
            return instance;
        }

        ~CidaModule()
        {
            this.Dispose();
        }

        public static CidaModule Extract(string path)
        {
            return Extract(ExtractFiles(path));
        }
        
        public static CidaModule Extract(byte[] module)
        {
            return Extract(ExtractFiles(module));
        }
        
        private static CidaModule Extract(IDictionary<string, Stream> fileStreams)
        {
            var parsedMetadata = ParseMetadata(fileStreams);

            return new CidaModule(parsedMetadata, fileStreams);
        }

        public static CidaModule Unpacked(string path)
        {
            var fileStreams = Directory.GetFiles(path)
                .Select(x => (Path.GetRelativePath(path, x),
                    new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.Read) as Stream))
                .ToDictionary(x => x.Item1, x => x.Item2);

            var parsedMetadata = ParseMetadata(fileStreams);

            return new CidaModule(parsedMetadata, fileStreams);
        }

        public static CidaModule FromByteArray(byte[] module)
        {
            var fileStreams = ExtractFiles(module);

            var parsedMetadata = ParseMetadata(fileStreams);
            
            return new CidaModule(parsedMetadata, );
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
            using var archive = ZipFile.OpenRead(path);
            return ExtractFiles(archive);
        }

        private static IDictionary<string, Stream> ExtractFiles(byte[] module)
        {
            using var stream = new MemoryStream(module);
            using var archive = new ZipArchive(stream);
            return ExtractFiles(archive);
        }

        private static IDictionary<string, Stream> ExtractFiles(ZipArchive archive)
        {
            IDictionary<string, Stream> fileStreams = new Dictionary<string, Stream>();
            
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

        public async Task<IEnumerable<KeyValuePair<string, byte[]>>> Serialize()
        {
            var result = new List<KeyValuePair<string, byte[]>>();
            foreach (var moduleFile in this.moduleFiles)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await moduleFile.Value.CopyToAsync(memoryStream);

                    result.Add(new KeyValuePair<string, byte[]>(moduleFile.Key, memoryStream.ToArray()));
                }

                if (moduleFile.Value.CanSeek)
                {
                    moduleFile.Value.Seek(0, SeekOrigin.Begin);
                }
            }

            return result;
        }
    }
}