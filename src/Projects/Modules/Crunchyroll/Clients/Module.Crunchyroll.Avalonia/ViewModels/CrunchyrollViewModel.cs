using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using Grpc.Core;
using Module.Crunchyroll.Avalonia.Services;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CrunchyrollViewModel : ModuleViewModel
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private readonly IImageDownloadService imageDownloadService;
        private SeriesDetailViewModel selectedItem;
        private string searchTerm;
        private DispatcherTimer searchTermChangeTimer;
        private bool isSearching;
        private CancellationTokenSource searchTokenSource;
        private SemaphoreSlim cancellationSemaphore;

        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                this.RaiseAndSetIfChanged(ref this.searchTerm, value);
                this.DelaySearchTermChange();
            }
        }

        public string SearchStatus =>
            this.IsSearching ? "Searching ..." : !this.SearchResults.Any() ? "No results" : null;

        public AvaloniaList<SeriesDetailViewModel> SearchResults { get; } = new AvaloniaList<SeriesDetailViewModel>();

        public bool IsSearching
        {
            get => this.isSearching;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isSearching, value);
                this.RaisePropertyChanged(nameof(this.SearchStatus));
            }
        }

        public void DelaySearchTermChange()
        {
            this.searchTermChangeTimer.Stop();
            this.searchTermChangeTimer.Start();
        }

        public SeriesDetailViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
                this.OnSelectedItemChanged();
            }
        }

        private void OnSelectedItemChanged()
        {
            Task.Run(async () => { await this.SelectedItem.LoadAsync(); });
        }

        public CrunchyrollViewModel(CrunchyrollService.CrunchyrollServiceClient client,
            IImageDownloadService imageDownloadService)
        {
            this.client = client;
            this.imageDownloadService = imageDownloadService;

            this.searchTermChangeTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(500),
                DispatcherPriority.Normal,
                async (obj, args) =>
                {
                    this.searchTermChangeTimer.Stop();
                    var tokenSource = new CancellationTokenSource();
                    try
                    {
                        await this.cancellationSemaphore.WaitAsync();
                        this.searchTokenSource?.Cancel();
                        this.searchTokenSource?.Dispose();
                        this.searchTokenSource = tokenSource;
                    }
                    finally
                    {
                        this.cancellationSemaphore.Release();
                    }

                    this.IsSearching = true;
                    try
                    {
                        await this.Search(this.SearchTerm, tokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        this.IsSearching = false;
                    }
                });

            this.cancellationSemaphore = new SemaphoreSlim(1, 1);
        }

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        public override string Name => "Crunchyroll";


        public async Task Search(string searchTerm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return;
            }

            SearchResponse searchResult = null;

            try
            {
                searchResult = await this.client.SearchAsync(new SearchRequest()
                {
                    SearchTerm = searchTerm,
                }, cancellationToken: cancellationToken);
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Cancelled)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            var results = new List<SeriesDetailViewModel>();

            if (searchResult == null)
            {
                throw new OperationCanceledException();
            }

            foreach (var searchResultItem in searchResult.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results.Add(new SeriesDetailViewModel(this.client, this.imageDownloadService, searchResultItem));
            }

            this.SearchResults.Clear();
            this.SearchResults.AddRange(results);
            this.RaisePropertyChanged(nameof(this.SearchStatus));
        }
    }
}