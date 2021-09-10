using System.IO.Compression;
using System.Reflection;
using Cida.Api;

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

            var type = this.Assembly.GetType(this.Metadata.EntryType);

            // TODO: Replace this with custom exception
            this.entryType = type ?? throw new InvalidOperationException($"Entry type '{this.Metadata.EntryType}' not found in assembly");
        }

        private CidaModuleLoadContext InitializeLoadContext()
        {
            var context = new CidaModuleLoadContext();
            context.Resolving += (resolveContext, name) =>
            {
                if (!this.moduleFiles.TryGetValue($"{name.Name}.dll", out var resolvedDependency))
                {
                    // TODO: Replace this with custom exception
                    throw new InvalidOperationException($"Assembly '{name.Name}.dll' not found.");
                }

                return resolveContext.LoadFromStream(resolvedDependency);
            };

            return context;
        }

        public async Task<IModule?> Load(IDatabaseConnector databaseConnector, Cida.Api.IFtpClient ftpClient, Cida.Api.Models.Filesystem.Directory moduleDirectory, IModuleLogger moduleLogger)
        {
            var instance = (IModule?)Activator.CreateInstance(this.entryType);
            if (instance != null)
            {
                await instance.Load(databaseConnector, ftpClient, moduleDirectory, moduleLogger);
            }
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

        private static CidaModuleMetadata ParseMetadata(IDictionary<string, Stream> fileStreams)
        {
            if (!fileStreams.TryGetValue(PackagesInfo, out var metadata))
            {
                // TODO: Replace this with custom exception
                throw new InvalidOperationException($"No '{PackagesInfo}' found in root dir of module!");
            }

            using var metadataStream = new StreamReader(metadata, leaveOpen: true);
            var parsedMetadata = System.Text.Json.JsonSerializer.Deserialize<CidaModuleMetadata>(metadataStream.ReadToEnd());

            if(parsedMetadata == null)
            {
                throw new InvalidOperationException("Failed to deserialize module metadata");
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

        public async Task<MemoryStream> ToArchiveStream()
        {
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var (filePath, value) in this.moduleFiles)
                {
                    var entry = archive.CreateEntry(filePath);
                    await using var entryStream = entry.Open();
                    value.Seek(0, SeekOrigin.Begin);
                    value.CopyTo(entryStream);
                    value.Seek(0, SeekOrigin.Begin);
                }
            }

            return memoryStream;
        }

        public async Task<byte[]> ToArchiveBytes()
        {
            var stream = await this.ToArchiveStream();
            var result = stream.ToArray();
            stream.Close();
            return result;
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

        public async Task<byte[]> GetClientModule(Guid id)
        {
            if (this.Metadata.Clients.TryGetValue(id, out var clientModule))
            {
                var stream = this.moduleFiles[clientModule];
                await using var memoryStream = new MemoryStream();
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(memoryStream);
                stream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }

            return Array.Empty<byte>();
        }
    }
}