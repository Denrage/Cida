using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class SeriesDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private CollectionDetailViewModel selectedItem;
        public string Id { get; set; }

        public IBitmap Image { get; set; }

        public IBitmap Thumbnail { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public AvaloniaList<CollectionDetailViewModel> Collections { get; set; } =
            new AvaloniaList<CollectionDetailViewModel>();

        public CollectionDetailViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
                this.OnSelectedItemChanged();
                selectedItem = value; 
            }
        }

        private void OnSelectedItemChanged()
        {
            Task.Run(async () => { await this.SelectedItem.LoadAsync(); });
        }


        public SeriesDetailViewModel(CrunchyrollService.CrunchyrollServiceClient client)
        {
            this.client = client;
        }

        public async Task LoadAsync()
        {
            var collections = await this.client.GetCollectionsAsync(new CollectionsRequest() { Id = this.Id });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Collections.Clear();
                this.Collections.AddRange(collections.Collections.Select(x => new CollectionDetailViewModel(this.client) { Id = x.Id, Name = x.Name, Description = x.Description}));
            });
        }
    }
}
