using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CrunchyrollViewModel : ModuleViewModel
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private SeriesDetailViewModel selectedItem;
        private readonly ObservableAsPropertyHelper<IEnumerable<SeriesDetailViewModel>> searchResults;
        private readonly ObservableAsPropertyHelper<bool> showResults;

        private string searchTerm;
        public string SearchTerm
        {
            get => this.searchTerm;
            set => this.RaiseAndSetIfChanged(ref this.searchTerm, value);
        }

        private bool isSearchFocused;
        public bool IsSearchFocused
        {
            get => this.isSearchFocused;
            set => this.RaiseAndSetIfChanged(ref this.isSearchFocused, value);
        }


        public bool ShowResults => this.showResults.Value;

        public IEnumerable<SeriesDetailViewModel> SearchResults => this.searchResults.Value;

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
            Task.Run(async () =>
            {
                await this.SelectedItem.LoadAsync();
            });
        }

        public CrunchyrollViewModel(CrunchyrollService.CrunchyrollServiceClient client)
        {
            this.client = client;
            this.searchResults = this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Where(x => !string.IsNullOrEmpty(x))
                .SelectMany(this.Search)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.SearchResults);

            this.showResults = this.WhenAnyValue(x => x.SearchResults, x => x.IsSearchFocused)
                .Where(x => x.Item1 != null)
                .Select(x => x.Item1.Any() && x.Item2)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.ShowResults);

            this.searchResults.ThrownExceptions.Subscribe(x => Debugger.Log(0, "", ""));
            this.showResults.ThrownExceptions.Subscribe(x => Debugger.Log(0, "", ""));
        }

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        public override string Name => "Crunchyroll";


        public async Task<IEnumerable<SeriesDetailViewModel>> Search(string searchTerm)
        {
            var searchResult = await this.client.SearchAsync(new SearchRequest()
            {
                SearchTerm = searchTerm,
            });

            var results = new List<SeriesDetailViewModel>();

            foreach (var searchResultItem in searchResult.Items)
            {
                results.Add(new SeriesDetailViewModel(this.client)
                {
                    Name = searchResultItem.Name,
                    Thumbnail = await this.DownloadImageAsync(searchResultItem.PortraitImage?.Medium ?? searchResultItem.LandscapeImage?.Thumbnail ?? "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg"),
                    Image = await this.DownloadImageAsync(searchResultItem.PortraitImage?.Full ?? searchResultItem.LandscapeImage?.Large ?? "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg"),
                    Description = searchResultItem.Description,
                    Id = searchResultItem.Id,
                });
            }

            return results;
        }

        private async Task<IBitmap> DownloadImageAsync(string url)
        {
            var request = WebRequest.Create(new Uri(url, UriKind.Absolute));
            request.Timeout = -1;
            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            if (responseStream is null)
            {
                throw new InvalidOperationException();
            }

            var memoryStream = new MemoryStream();

            await responseStream.CopyToAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return new Bitmap(memoryStream);
        }
    }
}
