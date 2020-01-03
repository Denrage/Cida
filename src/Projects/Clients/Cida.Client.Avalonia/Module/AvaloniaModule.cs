using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Cida.Client.Avalonia.Api;

namespace Cida.Client.Avalonia.Module
{
    public class AvaloniaModule
    {
        private const string PackagesInfo = "PackagesInfo.json";
        private readonly IDictionary<string, Stream> moduleFiles;
        private readonly CidaAvaloniaModuleLoadContext loadContext;
        private readonly Type entryType;

        public Assembly Assembly { get; }

        public AvaloniaModuleMetadata Metadata { get; }

        public IModule Module { get; }

        public AvaloniaModule(
            AvaloniaModuleMetadata metadata,
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

            // Hack: Don't use static
            ViewLocator.Assemblies.Add(this.Assembly);

            this.entryType = this.Assembly.GetType(this.Metadata.EntryType);
            if (this.entryType is null)
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"Entry type '{this.Metadata.EntryType}' not found in assembly");
            }

            this.Module = (IModule)Activator.CreateInstance(this.entryType);
        }

        private CidaAvaloniaModuleLoadContext InitializeLoadContext()
        {
            var loadContext = new CidaAvaloniaModuleLoadContext();
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

        ~AvaloniaModule()
        {
            this.Dispose();
        }

        public static AvaloniaModule Extract(string path)
        {
            return Extract(ExtractFiles(path));
        }

        public static AvaloniaModule Extract(byte[] module)
        {
            return Extract(ExtractFiles(module));
        }

        private static AvaloniaModule Extract(IDictionary<string, Stream> fileStreams)
        {
            var parsedMetadata = ParseMetadata(fileStreams);

            return new AvaloniaModule(parsedMetadata, fileStreams);
        }

        public static AvaloniaModule Unpacked(string path)
        {
            var fileStreams = Directory.GetFiles(path)
                .Select(x => (Path.GetRelativePath(path, x),
                    new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.Read) as Stream))
                .ToDictionary(x => x.Item1, x => x.Item2);

            var parsedMetadata = ParseMetadata(fileStreams);

            return new AvaloniaModule(parsedMetadata, fileStreams);
        }

        private static AvaloniaModuleMetadata ParseMetadata(IDictionary<string, Stream> fileStreams)
        {
            if (!fileStreams.TryGetValue(PackagesInfo, out var metadata))
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"No '{PackagesInfo}' found in root dir of module!");
            }
#nullable enable
            AvaloniaModuleMetadata? parsedMetadata = null;
#nullable disable
            using (var metadataStream = new StreamReader(metadata, leaveOpen: true))
            {
                var json = metadataStream.ReadToEnd();
                parsedMetadata = System.Text.Json.JsonSerializer.Deserialize<AvaloniaModuleMetadata>(json);
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
    }
}
