using Cida.Client.WebApp.Api.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cida.Client.WebApp.Module
{
    public class WebModule
    {
        private const string packeInformationFile = "PackagesInfo.json";
        private readonly CidaBlazorModuleLoadContext loadContext;
        private Type entryType;

        public WebModuleMetadata Metadata { get; set; }

        private Assembly Assembly { get; set; }

        private IDictionary<string, Stream> moduleFiles;

        public ModuleViewBase ModuleView { get; set; }

        public WebModule(WebModuleMetadata metadata,
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

            this.ModuleView = (ModuleViewBase)Activator.CreateInstance(this.entryType);
        }

        private CidaBlazorModuleLoadContext InitializeLoadContext()
        {
            var loadContext = new CidaBlazorModuleLoadContext();
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

        ~WebModule()
        {
            this.Dispose();
        }

        public static WebModule Extract(string path)
        {
            return Extract(ExtractFiles(path));
        }

        public static WebModule Extract(byte[] module)
        {
            return Extract(ExtractFiles(module));
        }

        private static WebModule Extract(IDictionary<string, Stream> fileStreams)
        {
            var parsedMetadata = ParseMetadata(fileStreams);

            return new WebModule(parsedMetadata, fileStreams);
        }

        public static WebModule Unpacked(string path)
        {
            var fileStreams = Directory.GetFiles(path)
                .Select(x => (Path.GetRelativePath(path, x),
                    new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.Read) as Stream))
                .ToDictionary(x => x.Item1, x => x.Item2);

            var parsedMetadata = ParseMetadata(fileStreams);

            return new WebModule(parsedMetadata, fileStreams);
        }

        private static WebModuleMetadata ParseMetadata(IDictionary<string, Stream> fileStreams)
        {
            if (!fileStreams.TryGetValue(packeInformationFile, out var metadataStream))
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"No '{packeInformationFile}' found in root dir of module!");
            }
#nullable enable
            WebModuleMetadata? parsedMetadata = null;
#nullable disable
            using (var metadataStreamReader = new StreamReader(metadataStream, leaveOpen: true))
            {
                var json = metadataStreamReader.ReadToEnd();
                parsedMetadata = System.Text.Json.JsonSerializer.Deserialize<WebModuleMetadata>(json);
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
