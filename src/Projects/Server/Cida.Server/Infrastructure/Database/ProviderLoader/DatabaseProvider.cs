using Cida.Api.Database;
using System.IO.Compression;
using System.Reflection;

namespace Cida.Server.Infrastructure.Database.ProviderLoader;

public class DatabaseProvider
{
    private const string PackagesInfo = "PackagesInfo.json";
    private readonly IDictionary<string, Stream> providerFiles;
    private readonly DatabaseProviderLoadContext loadContext;

    public Assembly Assembly { get; }

    public DatabaseProviderMetadata Metadata { get; }

    public IDatabaseProvider Provider { get; }

    public DatabaseProvider(
        DatabaseProviderMetadata metadata,
        IDictionary<string, Stream> moduleFiles)
    {
        Type? entryType = null;
        Metadata = metadata;
        this.providerFiles = moduleFiles;
        loadContext = InitializeLoadContext();

        if (!this.providerFiles.TryGetValue(Metadata.AssemblyFile, out var assembly))
        {
            // TODO: Replace this with custom exception
            throw new InvalidOperationException($"Assembly '{Metadata.AssemblyFile}' not found!");
        }

        Assembly = loadContext.LoadFromStream(assembly);

        entryType = Assembly.GetType(Metadata.EntryType);
        if (entryType is null)
        {
            // TODO: Replace this with custom exception
            throw new InvalidOperationException($"Entry type '{Metadata.EntryType}' not found in assembly");
        }

        Provider = (IDatabaseProvider)Activator.CreateInstance(entryType)!;
    }

    private DatabaseProviderLoadContext InitializeLoadContext()
    {
        var context = new DatabaseProviderLoadContext();
        context.Resolving += (resolveContext, name) =>
        {
            if (!providerFiles.TryGetValue($"{name.Name}.dll", out var resolvedDependency))
            {
                    // TODO: Replace this with custom exception
                    throw new InvalidOperationException($"Assembly '{name.Name}.dll' not found.");
            }

            return resolveContext.LoadFromStream(resolvedDependency);
        };

        return context;
    }

    ~DatabaseProvider()
    {
        Dispose();
    }

    public static DatabaseProvider Extract(string path)
    {
        return Extract(ExtractFiles(path));
    }

    public static DatabaseProvider Extract(byte[] module)
    {
        return Extract(ExtractFiles(module));
    }

    private static DatabaseProvider Extract(IDictionary<string, Stream> fileStreams)
    {
        var parsedMetadata = ParseMetadata(fileStreams);

        return new DatabaseProvider(parsedMetadata, fileStreams);
    }

    public static DatabaseProvider Unpacked(string path)
    {
        var fileStreams = Directory.GetFiles(path)
            .Select(x => (Path.GetRelativePath(path, x),
                new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.Read) as Stream))
            .ToDictionary(x => x.Item1, x => x.Item2);

        var parsedMetadata = ParseMetadata(fileStreams);

        return new DatabaseProvider(parsedMetadata, fileStreams);
    }

    private static DatabaseProviderMetadata ParseMetadata(IDictionary<string, Stream> fileStreams)
    {
        if (!fileStreams.TryGetValue(PackagesInfo, out var metadata))
        {
            // TODO: Replace this with custom exception
            throw new InvalidOperationException($"No '{PackagesInfo}' found in root dir of module!");
        }
        DatabaseProviderMetadata? parsedMetadata = null;
        using (var metadataStream = new StreamReader(metadata, leaveOpen: true))
        {
            var json = metadataStream.ReadToEnd();
            parsedMetadata = System.Text.Json.JsonSerializer.Deserialize<DatabaseProviderMetadata>(json);
        }

        return parsedMetadata!;
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
        loadContext.Unload();
        foreach (var file in providerFiles.Values)
        {
            file.Close();
        }

        GC.SuppressFinalize(this);
    }
}
