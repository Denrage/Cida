using Cida.Client.Avalonia.Api;
using Module.IrcAnime.Avalonia.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    [DebuggerDisplay("{pack.Name}")]
    public class DownloadContext : ViewModelBase
    {
        private long downloadedBytes;

        public event Action<DownloadContext> OnDownload;

        public Pack Pack { get; }

        public long DownloadedBytes
        {
            get => downloadedBytes;

            set
            {
                this.RaiseAndSetIfChanged(ref this.downloadedBytes, value);
                this.RaisePropertyChanged(nameof(this.Progress));
                this.RaisePropertyChanged(nameof(this.NotDownloaded));
                this.RaisePropertyChanged(nameof(this.Downloaded));
            }
        }

        public int Progress =>
            !this.NotDownloaded &&
            !this.Downloaded ? (int)Math.Round((double)this.DownloadedBytes * 100.0 / (double)this.Pack.Size) : 0;

        public bool Downloaded => this.DownloadedBytes == this.Pack.Size;

        public bool NotDownloaded => this.DownloadedBytes == -1;

        public DownloadContext(Pack pack)
        {
            this.Pack = pack;
        }

        public async Task Download()
        {
            await Task.Run(() => this.OnDownload.Invoke(this));
        }
    }
}
