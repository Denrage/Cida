using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Horriblesubs;
using Module.HorribleSubs.Avalonia.Services;
using ReactiveUI;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Module.HorribleSubs.Avalonia.ViewModels
{
    public class HorribleSubsViewModel : ModuleViewModel
    {
        private readonly HorribleSubsService.HorribleSubsServiceClient client;
        private readonly DownloadStatusService downloadStatusService;
        private string searchTerm;


        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                this.RaiseAndSetIfChanged(ref this.searchTerm, value);
            }
        }

        public AvaloniaList<PackItem> Packs { get; } = new AvaloniaList<PackItem>();

        public HorribleSubsViewModel(HorribleSubsService.HorribleSubsServiceClient client, DownloadStatusService downloadStatusService)
        {
            this.client = client;
            this.downloadStatusService = downloadStatusService;
            this.downloadStatusService.OnStatusUpdate += async () =>
            {

                foreach (var pack in this.Packs)
                {
                    var status = await this.downloadStatusService.GetStatus(pack.OriginalName);
                    if (status != null)
                    {
                        pack.Size = (long)status.Filesize;
                        pack.DownloadedBytes = !status.Downloaded ? (long)status.DownloadedBytes : (long)status.Filesize;
                    }
                }
            };
        }

        public override string Name { get; } = "Horrible Subs";

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        public async Task Search()
        {
            if (!string.IsNullOrEmpty(this.SearchTerm))
            {
                var result = await this.client.SearchAsync(new SearchRequest()
                {
                    SearchTerm = this.SearchTerm
                });


                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.Packs.Clear();
                    this.Packs.AddRange(result.SearchResults.Select(x => new PackItem(x.FileName, x.PackageNumber.ToString(), x.BotName, x.FileSize, -1)));
                });

            }
        }
    }

    public class PackItem : ReactiveObject
    {
        private static readonly Regex ResolutionRegex = new Regex("(480p|720p|1080p)");
        private static readonly Regex BracketsRegex = new Regex("\\[.*?\\]");
        private static readonly Regex BracesRegex = new Regex("\\(.*?\\)");
        private long downloadedBytes;

        public string Name { get; }

        public string OriginalName { get; }

        public string PackNumber { get; }

        public string Bot { get; }

        public string Resolution { get; }

        public long Size { get; set; }

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
            !this.Downloaded ? (int)Math.Round((double)this.DownloadedBytes * 100.0 / (double)this.Size) : 0;

        public bool Downloaded => this.DownloadedBytes == this.Size;

        public bool NotDownloaded => this.DownloadedBytes == -1;

        public PackItem(string name, string packNumber, string bot, long size, long downloadedBytes)
        {
            this.OriginalName = name;
            this.Name = name;
            foreach (var match in BracesRegex.Matches(this.Name).Concat(BracketsRegex.Matches(this.Name)))
            {
                this.Name = this.Name.Replace(match.ToString(), string.Empty);
            }

            this.Name = this.Name.Trim();

            this.Resolution = ResolutionRegex.Match(name).Value;

            this.PackNumber = packNumber;
            this.Bot = bot;
            this.Size = size;
            this.DownloadedBytes = downloadedBytes;
        }
    }
}
