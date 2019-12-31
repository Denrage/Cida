using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using Grpc.Core;
using Module.Crunchyroll.Avalonia.ViewModels;

namespace Module.Crunchyroll.Avalonia
{
    public class Module : IModule
    {
        private CrunchyrollService.CrunchyrollServiceClient client;
        public string Name => "Crunchyroll";

        public ModuleViewModel ViewModel { get; private set; }

        public async Task LoadAsync(Channel channel)
        {
            this.client = new CrunchyrollService.CrunchyrollServiceClient(channel);
            this.ViewModel = new CrunchyrollViewModel(this.client);
            await Task.CompletedTask;
        }
    }
}