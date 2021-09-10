namespace Cida.Api.Models.Filesystem;

public class File : DirectoryItem
{
    public static File EmptyFile = new("Empty", Directory.EmptyDirectory, null, null);

    private Func<CancellationToken, Task<Stream>> getStream;

    public File(string name, Directory directory, Func<CancellationToken, Task<Stream>> getStream, Action onDispose = null)
        : base(name, directory, onDispose)
    {
        this.getStream = getStream;
    }

    public File ReplaceContent(Func<CancellationToken, Task<Stream>> getStream, Action onDispose = null)
    {
        if (this.Disposed)
        {
            throw new InvalidOperationException("This file is already disposed");
        }

        var file = new File(this.Name, this.Directory, getStream, onDispose);
        this.Dispose();
        return file;
    }

    public async Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
    {
        if (this.Disposed)
        {
            throw new InvalidOperationException("This file is already disposed");
        }

        return await this.getStream(cancellationToken);
    }

    public async Task CopyToAsync(File file, CancellationToken cancellationToken)
    {
        using var stream = await this.GetStreamAsync(cancellationToken);
        using var otherStream = await file.GetStreamAsync(cancellationToken);

        stream.CopyTo(otherStream);
    }
}
