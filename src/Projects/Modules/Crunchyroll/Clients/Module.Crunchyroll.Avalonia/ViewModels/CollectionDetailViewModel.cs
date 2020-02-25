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

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Episodes.Clear();
                this.Episodes.AddRange(episodes.Episodes.Select(x => new EpisodeDetailViewModel(this.client) { EpisodeNumber = x.EpisodeNumber, Name = x.Name, Description = x.Description, Id = x.Id}));
            });
        }
    }
}
