using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using Grpc.Core;
using Module.Crunchyroll.Avalonia.Services;
using Module.Crunchyroll.Avalonia.ViewModels;

namespace Module.Crunchyroll.Avalonia
{
    public class Module : IModule
    {
        public const string ImageUnavailable =
            "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg";
        private CrunchyrollService.CrunchyrollServiceClient client;
        public string Name => "Crunchyroll";

        public ModuleViewModel ViewModel { get; private set; }

        public async Task LoadAsync(Channel channel, ISettingsFactory settingsFactory)
        {
            this.client = new CrunchyrollService.CrunchyrollServiceClient(channel);
            this.ViewModel = new CrunchyrollViewModel(this.client, new ImageDownlodService());
            await Task.CompletedTask;
        }
    }
}