using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CollectionDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
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


        public CollectionDetailViewModel(CrunchyrollService.CrunchyrollServiceClient client)
        {
            this.client = client;
        }

        public async Task LoadAsync()
        {
            var episodes = await this.client.GetEpisodesAsync(new EpisodeRequest() { Id = this.Id });

            await Dispatcher.UIThread.InvokeAsync( () =>
            {
                this.Episodes.Clear();

                foreach (var episode in episodes.Episodes)
                {
                    this.Episodes.Add(new EpisodeDetailViewModel(this.client)
                    {
                        EpisodeNumber = episode.EpisodeNumber,
                        Name = episode.Name,
                        Description = episode.Description,
                        Id = episode.Id,
                        ImageUrl = episode.Image?.Thumbnail ?? "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg"
                    });
                }
            });
        }
    }
}
