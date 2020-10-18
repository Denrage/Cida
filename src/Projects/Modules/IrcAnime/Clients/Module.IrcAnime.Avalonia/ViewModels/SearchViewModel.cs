using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Ircanime;
using Module.IrcAnime.Avalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly DownloadContextService downloadContextService;
        private readonly SemaphoreSlim packSemaphore = new SemaphoreSlim(1, 1);
        private string searchTerm;
        private string selectedResolution;
        private string selectedBot;

        AvaloniaList<DownloadContext> Packs { get; } = new AvaloniaList<DownloadContext>();

        IEnumerable<DownloadContext> FilteredPacks => this.Packs.Where(x => x.Pack.Resolutions.FirstOrDefault(y => y.ResolutionType == this.SelectedResolution) != null && x.Pack.Resolutions.First(y => y.ResolutionType == this.SelectedResolution).Bots.FirstOrDefault(x => x.Name == this.SelectedBot) != null);

        public string SelectedResolution
        {
            get => selectedResolution;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedResolution, value);
                this.RaisePropertyChanged(nameof(this.FilteredPacks));
            }
        }

        public string SelectedBot
        {
            get => selectedBot;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedBot, value);
                this.RaisePropertyChanged(nameof(this.FilteredPacks));
            }
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

        public SearchViewModel(IrcAnimeService.IrcAnimeServiceClient client, DownloadContextService downloadContextService)
        {
            this.client = client;
            this.downloadContextService = downloadContextService;
            this.Packs.CollectionChanged += (s,e) => this.RaisePropertyChanged(nameof(this.FilteredPacks));
        }

        public async Task Search()
        {
            if (!string.IsNullOrEmpty(this.SearchTerm))
            {
                var result = await this.client.SearchAsync(new SearchRequest()
                {
                    SearchTerm = this.SearchTerm
                });

                var contexts = new List<DownloadContext>();
                var bots = new List<string>();
                var resolutions = new List<string>();

                foreach (var item in result.SearchResults)
                {
                    var context = await this.downloadContextService.GetContextAsync(new Models.PackMetadata()
                    {
                        Bot = item.BotName,
                        Name = item.FileName,
                        Number = (ulong)item.PackageNumber,
                        Size = item.FileSize,
                    });

                    context.OnDownload += async item => await this.Context_OnDownload(item);

                    foreach (var resolution in context.Pack.Resolutions)
                    {
                        if (!resolutions.Contains(resolution.ResolutionType))
                        {
                            resolutions.Add(resolution.ResolutionType);
                        }

                        foreach (var bot in resolution.Bots)
                        {
                            if (!bots.Contains(bot.Name))
                            {
                                bots.Add(bot.Name);
                            }
                        }
                    }

                    if (contexts.FirstOrDefault(x => x.Pack.Identifier == context.Pack.Identifier) == null)
                    {
                        contexts.Add(context);
                    }
                }

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

                        this.Bots.AddRange(bots.OrderBy(x => x));
                        this.Resolutions.AddRange(resolutions.OrderBy(x => x));

                        this.SelectedBot = this.Bots.FirstOrDefault();
                        this.SelectedResolution = this.Resolutions.FirstOrDefault();

                        this.Packs.AddRange(contexts);
                    }
                    finally
                    {
                        this.packSemaphore.Release();
                    }
                });

            }
        }

        private async Task Context_OnDownload(DownloadContext item)
        {
            var resolution = item.Pack.Resolutions.FirstOrDefault(x => x.ResolutionType == this.SelectedResolution);

            if (resolution != null)
            {
                var bot = resolution.Bots.FirstOrDefault(x => x.Name == this.SelectedBot);

                if (bot != null)
                {
                    var group = item.Pack.Resolutions.FirstOrDefault(x => x.ResolutionType == this.SelectedResolution).Bots.FirstOrDefault(x => x.Name == this.SelectedBot).UploaderGroup.Last();
                    await this.client.DownloadAsync(new DownloadRequest()
                    {
                        DownloadRequest_ = new DownloadRequest.Types.Request()
                        {
                            BotName = this.SelectedBot,
                            FileName = group.Filename,
                            PackageNumber = (long)group.PackageNumber,
                        }
                    });
                }
            }
        }
    }
}
