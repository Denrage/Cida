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
        private readonly DownloadService downloadService;
        private readonly SemaphoreSlim packSemaphore = new SemaphoreSlim(1, 1);
        private string searchTerm;

        AvaloniaList<DownloadContext> Packs { get; } = new AvaloniaList<DownloadContext>();

        public string SearchTerm
        {
            get => this.searchTerm;
            set => this.RaiseAndSetIfChanged(ref this.searchTerm, value);
        }

        public SearchViewModel(IrcAnimeService.IrcAnimeServiceClient client, DownloadContextService downloadContextService, DownloadService downloadService)
        {
            this.client = client;
            this.downloadContextService = downloadContextService;
            this.downloadService = downloadService;
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
                    var context = this.downloadContextService.GetContext(new Models.PackMetadata()
                    {
                        Bot = item.BotName,
                        Name = item.FileName,
                        Number = (ulong)item.PackageNumber,
                        Size = (ulong)item.FileSize,
                    });

                    context.OnDownload += async item => await this.Context_OnDownload(item);
                    context.OnDownloadLocally += async item => await this.Context_OnDownloadLocally(item);

                    if (contexts.FirstOrDefault(x => x.Pack.Name == context.Pack.Name) == null)
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
            await this.client.DownloadAsync(new DownloadRequest()
            {
                DownloadRequest_ = new DownloadRequest.Types.Request()
                {
                    BotName = item.Pack.Packs.First().Key,
                    FileName = item.Pack.Name,
                    PackageNumber = (long)item.Pack.Packs.First().Value,
                }
            });
        }

        private async Task Context_OnDownloadLocally(DownloadContext item)
        {
            await this.downloadService.Download(item.Pack.Name, default);
            await item.UpdateLocallyAvailable();
        }
    }
}
