using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Horriblesubs;
using Grpc.Core;
using Module.HorribleSubs.Avalonia.ViewModels;

namespace Module.HorribleSubs.Avalonia
{
    public class Module : IModule
    {
        public const string ImageUnavailable =
            "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg";
        private HorribleSubsService.HorribleSubsServiceClient client;
        public string Name => "Horrible Subs";

        public ModuleViewModel ViewModel { get; private set; }

        public async Task LoadAsync(Channel channel)
        {
            client = new HorribleSubsService.HorribleSubsServiceClient(channel);
            ViewModel = new HorribleSubsViewModel(client);
            await Task.CompletedTask;
        }
    }
}