using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CrunchyrollViewModel : ModuleViewModel
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private SearchResult selectedItem;

        public string SearchTerm { get; set; } = "Hello World";

        public AvaloniaList<SearchResult> SearchResults { get; } = new AvaloniaList<SearchResult>();

        public SearchResult SelectedItem
        {
            get => selectedItem;
            set {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
                this.OnSelectedItemChanged();
            }
        }

        private void OnSelectedItemChanged()
        {
            Task.Run(async () =>
            {
                var episodes = await this.client.GetEpisodesAsync(new EpisodeRequest(){ Id = this.SelectedItem.Id});
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.SelectedItem.Episodes.Clear();
                    this.SelectedItem.Episodes.AddRange(episodes.Episodes);
                });
            });
        }

        public CrunchyrollViewModel(CrunchyrollService.CrunchyrollServiceClient client)
        {
            this.client = client;
        }

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }

        public override string Name => "Crunchyroll";

        public async void Search()
        {
            var searchResult = await this.client.SearchAsync(new SearchRequest()
            {
                SearchTerm = this.SearchTerm,
            });

            this.SearchResults.Clear();
            foreach (var searchResultItem in searchResult.Items)
            {
                this.SearchResults.Add(new SearchResult()
                {
                    Name = searchResultItem.Name,
                    Thumbnail = await this.DownloadImageAsync(searchResultItem.PortraitImage?.Medium ?? searchResultItem.LandscapeImage?.Small ?? "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg"),
                    Image = await this.DownloadImageAsync(searchResultItem.PortraitImage?.Full ?? searchResultItem.LandscapeImage?.Large ?? "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg"),
                    Description = searchResultItem.Description,
                    Id = searchResultItem.Id,
                });
            }
        }

        public class SearchResult
        {
            public string Id { get; set; }

            public IBitmap Image { get; set; }

            public IBitmap Thumbnail { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public AvaloniaList<EpisodeResponse.Types.EpisodeItem> Episodes { get; set; } = new AvaloniaList<EpisodeResponse.Types.EpisodeItem>();
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
