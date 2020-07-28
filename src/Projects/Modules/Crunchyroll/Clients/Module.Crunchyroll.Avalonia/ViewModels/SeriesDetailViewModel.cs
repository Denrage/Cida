using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using Module.Crunchyroll.Avalonia.Services;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class SeriesDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private readonly IImageDownloadService imageDownloadService;
        private readonly SearchResponse.Types.SearchItem model;
        private CollectionDetailViewModel selectedItem;
        private IBitmap image;
        private IBitmap thumbnail;

        public string Id
        {
            get => this.model.Id;
            set => this.model.Id = value;
        }

        public IBitmap Image
        {
            get => this.image;
            private set => this.RaiseAndSetIfChanged(ref this.image, value);
        }

        public IBitmap Thumbnail
        {
            get => this.thumbnail;
            private set => this.RaiseAndSetIfChanged(ref this.thumbnail, value);
        }

        public string Name
        {
            get => this.model.Name;
            set => this.model.Name = value;
        }

        public string Description
        {
            get => this.model.Description;
            set => this.model.Description = value;
        }

        public AvaloniaList<CollectionDetailViewModel> Collections { get; set; } =
            new AvaloniaList<CollectionDetailViewModel>();

        public CollectionDetailViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
                this.OnSelectedItemChanged();
                this.selectedItem = value;
            }
        }

        private void OnSelectedItemChanged()
        {
            Task.Run(async () => { await this.SelectedItem.LoadAsync(); });
        }


        public SeriesDetailViewModel(CrunchyrollService.CrunchyrollServiceClient client,
            IImageDownloadService imageDownloadService, SearchResponse.Types.SearchItem model)
        {
            this.client = client;
            this.imageDownloadService = imageDownloadService;
            this.model = model;

            this.imageDownloadService
                .DownloadImageAsync(this.model.PortraitImage?.Full ??
                                    this.model.LandscapeImage?.Large ??
                                    Module.ImageUnavailable,
                    bitmap => this.Image = bitmap);

            this.imageDownloadService
                .DownloadImageAsync(this.model.PortraitImage?.Medium ??
                                    this.model.LandscapeImage?.Thumbnail ??
                                    Module.ImageUnavailable,
                    bitmap => this.Thumbnail = bitmap);
        }

        public async Task LoadAsync()
        {
            var collections = await this.client.GetCollectionsAsync(new CollectionsRequest() {Id = this.Id});
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Collections.Clear();
                this.Collections.AddRange(collections.Collections.Select(x =>
                    new CollectionDetailViewModel(this.client, this.imageDownloadService)));
            });
        }
    }
}