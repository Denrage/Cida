using Cida.Client.Avalonia.Api;
using Module.IrcAnime.Avalonia.Models;
using Module.IrcAnime.Avalonia.Services;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    [DebuggerDisplay("{Pack.Name}")]
    public class DownloadContext : ViewModelBase
    {
        private long downloadedBytes;
        private PackageStatus status = PackageStatus.NotAvailable;
        private readonly IModuleSettingsService moduleSettingsService;

        public event Action<DownloadContext> OnDownload;
        public event Action<DownloadContext> OnDownloadLocally;

        public Pack Pack { get; }

        public long DownloadedBytes
        {
            get => downloadedBytes;

            set
            {
                this.RaiseAndSetIfChanged(ref this.downloadedBytes, value);
                this.RaisePropertyChanged(nameof(this.Progress));
            }
        }

        public int Progress => (int)Math.Round((double)this.DownloadedBytes * 100.0 / (double)this.Pack.Size);

        public PackageStatus Status 
        { 
            get => this.status; 
            private set => this.RaiseAndSetIfChanged(ref this.status, value); 
        }

        public DownloadContext(Pack pack, IModuleSettingsService moduleSettingsService)
        {
            this.Pack = pack;
            this.moduleSettingsService = moduleSettingsService;

            Task.Run(async () =>
            {
                await this.UpdateLocallyAvailable();
            });
        }

        private async Task<string> GetDownloadFolderAsync() => (await this.moduleSettingsService.Get<DownloadService.DownloadSettings>()).DownloadFolder;

        public async Task Download() => await Task.Run(() => this.OnDownload?.Invoke(this));

        public async Task DownloadLocally() => await Task.Run(() => this.OnDownloadLocally?.Invoke(this));

        public async Task UpdateLocallyAvailable()
        {
            var downloadFolder = await this.GetDownloadFolderAsync();
            if (File.Exists(Path.Combine(downloadFolder, this.Pack.Name)))
            {
                this.SetDownloadedLocally();
            }
        }

        public void SetDownloading()
        {
            this.Status |= PackageStatus.Downloading;
        }

        public void SetDownloadedToCida()
        {
            this.Status = this.Status & ~PackageStatus.Downloading & ~PackageStatus.NotAvailable | PackageStatus.Cida;
        }

        public void SetDownloadedLocally()
        {
            this.Status = this.Status & ~PackageStatus.Downloading & ~PackageStatus.NotAvailable | PackageStatus.Locally;
        }
    }


    [Flags]
    public enum PackageStatus
    {
        NotAvailable = 1,
        Downloading = 2,
        Cida = 4,
        Locally = 8,
    }
}
