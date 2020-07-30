using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using Module.Crunchyroll.Avalonia.Services;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CollectionDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private readonly IImageDownloadService imageDownloadService;
        private EpisodeDetailViewModel selectedItem;
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AvaloniaList<EpisodeDetailViewModel> Episodes { get; set; } = new AvaloniaList<EpisodeDetailViewModel>();

        public EpisodeDetailViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
            }
        }


        public CollectionDetailViewModel(CrunchyrollService.CrunchyrollServiceClient client, IImageDownloadService imageDownloadService)
        {
            this.client = client;
            this.imageDownloadService = imageDownloadService;
        }

        public async Task LoadAsync()
        {
            var episodes = await this.client.GetEpisodesAsync(new EpisodeRequest() { Id = this.Id });

            await Dispatcher.UIThread.InvokeAsync( () =>
            {
                this.Episodes.Clear();

                foreach (var episode in episodes.Episodes)
                {
                    this.Episodes.Add(new EpisodeDetailViewModel(this.client, this.imageDownloadService, episode));
                }
            });
        }
    }
}
