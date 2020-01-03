using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Cida.Client.Avalonia.Api;
using Crunchyroll;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CrunchyrollViewModel : ModuleViewModel
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;

        public string SearchTerm { get; set; } = "Hello World";

        public AvaloniaList<SearchResult> SearchResults { get; } = new AvaloniaList<SearchResult>();

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
                    Image = await this.DownloadImageAsync(searchResultItem.PortraitImage?.Full ?? searchResultItem.LandscapeImage?.Full ?? "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg"),
                });
            }
        }

        public class SearchResult
        {
            public IBitmap Image { get; set; }

            public string Name { get; set; }
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
