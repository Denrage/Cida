using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Horriblesubs;
using Module.HorribleSubs.Avalonia.Services;
using ReactiveUI;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Module.HorribleSubs.Avalonia.ViewModels
{
    public class HorribleSubsViewModel : ModuleViewModel
    {
        private readonly HorribleSubsService.HorribleSubsServiceClient client;
        private readonly DownloadStatusService downloadStatusService;
        private readonly List<PackItem> relevantUpdatingPacks = new List<PackItem>();
        private readonly SemaphoreSlim packSemaphore = new SemaphoreSlim(1, 1);
        private string searchTerm;
        private string selectedResolution;
        private string selectedBot;

        public string SelectedResolution
        {
            get => selectedResolution;
            set => this.RaiseAndSetIfChanged(ref this.selectedResolution, value);
        }

        public string SelectedBot
        {
            get => selectedBot;
            set => this.RaiseAndSetIfChanged(ref this.selectedBot, value);
        }

        public AvaloniaList<string> Bots { get; } = new AvaloniaList<string>();

        public AvaloniaList<string> Resolutions { get; } = new AvaloniaList<string>();


        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                this.RaiseAndSetIfChanged(ref this.searchTerm, value);
            }
        }

        public AvaloniaList<PackItem> AvailableDownloads { get; } = new AvaloniaList<PackItem>();

        public AvaloniaList<PackItem> Packs { get; } = new AvaloniaList<PackItem>();

        public HorribleSubsViewModel(HorribleSubsService.HorribleSubsServiceClient client, DownloadStatusService downloadStatusService)
        {
            this.client = client;
            this.downloadStatusService = downloadStatusService;
            this.downloadStatusService.OnStatusUpdate += async () =>
            {
                await this.packSemaphore.WaitAsync();

                try
                {
                    if (this.relevantUpdatingPacks.Count == 0)
                    {
                        this.relevantUpdatingPacks.AddRange(this.Packs);
                    }

                    var toRemoved = new List<PackItem>();

                    foreach (var pack in this.relevantUpdatingPacks)
                    {
                        var selectedBot = pack.Bots.FirstOrDefault(x => x.Name == this.SelectedBot);

                        if (selectedBot != null)
                        {
                            var selectedResolution = selectedBot.Resolutions.FirstOrDefault(x => x.Resolution == this.SelectedResolution);
                            if (selectedResolution != null)
                            {
                                var status = await this.downloadStatusService.GetStatus(selectedResolution.FullName);
                                if (status != null)
                                {
                                    pack.DownloadInformation.Size = (long)status.Filesize;
                                    pack.DownloadInformation.DownloadedBytes = !status.Downloaded ? (long)status.DownloadedBytes : (long)status.Filesize;

                                    if (status.Downloaded)
                                    {
                                        toRemoved.Add(pack);
                                    }
                                }
                            }
                        }

                    }

                    foreach (var toRemovedItem in toRemoved)
                    {
                        this.relevantUpdatingPacks.Remove(toRemovedItem);
                    }
                }
                finally
                {
                    this.packSemaphore.Release();
                }
            };
        }

        public override string Name { get; } = "Horrible Subs";

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        public async Task Download(PackItem pack)
        {
            var bot = pack.Bots.FirstOrDefault(x => x.Name == this.SelectedBot);

            if (bot != null)
            {
                var resolution = bot.Resolutions.FirstOrDefault(x => x.Resolution == this.SelectedResolution);

                if (resolution != null)
                {
                    await this.client.DownloadAsync(new DownloadRequest()
                    {
                        DownloadRequest_ = new DownloadRequest.Types.Request()
                        {
                            BotName = this.SelectedBot,
                            FileName = resolution.FullName,
                            PackageNumber = int.Parse(resolution.PackNumber),
                        }
                    });
                }
            }
        }


        public async Task RefreshAvailableDownloads()
        {
            var files = (await this.client.DownloadedFilesAsync(new DownloadedFilesRequest())).Files;
            var packItems = files.Select(x => new PackItem(x.Filename, (long)x.Filesize, null));

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                this.AvailableDownloads.Clear();
                this.AvailableDownloads.AddRange(packItems);
            });
        }


        public async Task Search()
        {
            if (!string.IsNullOrEmpty(this.SearchTerm))
            {
                var result = await this.client.SearchAsync(new SearchRequest()
                {
                    SearchTerm = this.SearchTerm
                });

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await this.packSemaphore.WaitAsync();
                    try
                    {
                        this.Packs.Clear();
                        this.SelectedBot = null;
                        this.SelectedResolution = null;
                        this.Bots.Clear();
                        this.Resolutions.Clear();
                        var bots = new List<string>();
                        var resolutions = new List<string>();
                        var packItems = PackItem.FromSearchResults(result.SearchResults, async packItem => await this.Download(packItem));

                        foreach (var pack in packItems)
                        {
                            foreach (var bot in pack.Bots)
                            {
                                if (!bots.Contains(bot.Name))
                                {
                                    bots.Add(bot.Name);
                                }

                                foreach (var resolution in bot.Resolutions)
                                {
                                    if (!resolutions.Contains(resolution.Resolution))
                                    {
                                        resolutions.Add(resolution.Resolution);
                                    }
                                }
                            }
                        }

                        this.Bots.AddRange(bots.OrderBy(x => x));
                        this.Resolutions.AddRange(resolutions.OrderBy(x => x));

                        this.SelectedBot = this.Bots.FirstOrDefault();
                        this.SelectedResolution = this.Resolutions.FirstOrDefault();

                        this.Packs.AddRange(packItems);
                    }
                    finally
                    {
                        this.packSemaphore.Release();
                    }
                });

            }
        }
    }

    [DebuggerDisplay("{Name}")]
    public class PackItem : ReactiveObject
    {
        private static readonly Regex SanitizeNameRegex = new Regex("(\\[.*?\\]|\\(.*?\\)|-|\\.mkv|\\.mp4|\\{.*?\\])");
        private static readonly Regex WhitespaceRegex = new Regex("[ ]{2,}");
        private readonly Func<PackItem, Task> onDownload;

        public AvaloniaList<BotInformation> Bots { get; } = new AvaloniaList<BotInformation>();

        public PackDownloadInformation DownloadInformation { get; }

        public string Name { get; }

        public PackItem(string name, long size, Func<PackItem, Task> onDownload)
        {
            this.DownloadInformation = new PackDownloadInformation()
            {
                DownloadedBytes = -1,
                Size = size,
            };

            this.Name = name;
            this.onDownload = onDownload;
        }

        public async Task Download()
        {
            await this.onDownload.Invoke(this);
        }

        public static string SanitizeName(string name)
        {
            name = SanitizeNameRegex.Replace(name, string.Empty);
            name = WhitespaceRegex.Replace(name, " ");
            name = name.Trim();

            return name;
        }

        public static PackItem[] FromSearchResults(IEnumerable<Horriblesubs.SearchResponse.Types.SearchResult> results, Func<PackItem, Task> onDownload)
        {
            var packItems = new Dictionary<string, PackItem>();

            foreach (var result in results)
            {
                var sanitizedName = SanitizeName(result.FileName);

                if (!packItems.TryGetValue(sanitizedName, out var packItem))
                {
                    packItem = new PackItem(sanitizedName, result.FileSize, onDownload);
                    packItems[sanitizedName] = packItem;
                }

                var botInformation = packItem.Bots.FirstOrDefault(x => x.Name == result.BotName);

                if (botInformation == null)
                {
                    botInformation = new BotInformation()
                    {
                        Name = result.BotName,
                    };
                    packItem.Bots.Add(botInformation);
                }

                var resolution = ResolutionInformation.GetResolution(result.FileName);

                var resolutionInformation = botInformation.Resolutions.FirstOrDefault(x => x.Resolution == resolution);

                if (resolutionInformation == null)
                {
                    resolutionInformation = new ResolutionInformation(result.FileName, result.PackageNumber.ToString());
                    botInformation.Resolutions.Add(resolutionInformation);
                }
            }

            return packItems.Values.ToArray();
        }

    }

    public class PackDownloadInformation : ReactiveObject
    {
        public long Size { get; set; }

        private long downloadedBytes;

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
    }

    [DebuggerDisplay("{Resolution}")]
    public class ResolutionInformation : ReactiveObject
    {
        private const string DefaultResolution = "480p";
        private static readonly Regex ResolutionRegex = new Regex("(480p|720p|1080p)");

        public string Resolution { get; }

        public string PackNumber { get; }

        public string FullName { get; set; }

        public ResolutionInformation(string fullName, string packNumber)
        {
            this.Resolution = GetResolution(fullName);
            this.FullName = fullName;
            this.PackNumber = packNumber;
        }

        public static string GetResolution(string fullName)
        {
            var result = ResolutionRegex.Match(fullName).Value;
            return string.IsNullOrEmpty(result) ? DefaultResolution : result;
        }
    }

    [DebuggerDisplay("{Name}")]
    public class BotInformation : ReactiveObject
    {
        public string Name { get; set; }

        public AvaloniaList<ResolutionInformation> Resolutions { get; } = new AvaloniaList<ResolutionInformation>();
    }
}
