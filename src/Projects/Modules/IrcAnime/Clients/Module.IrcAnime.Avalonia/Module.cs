using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Ircanime;
using Grpc.Core;
using Module.IrcAnime.Avalonia.ViewModels;

namespace Module.IrcAnime.Avalonia
{
    public class Module : IModule
    {
        public const string ImageUnavailable =
            "https://media.wired.com/photos/5a0201b14834c514857a7ed7/master/pass/1217-WI-APHIST-01.jpg";
        private IrcAnimeService.IrcAnimeServiceClient client;
        public string Name => "IRC Anime";

        public ModuleViewModel ViewModel { get; private set; }

        public async Task LoadAsync(Channel channel, ISettingsFactory settingsFactory)
        {
            var settingsService = settingsFactory.Get(this.Name);
            client = new IrcAnimeService.IrcAnimeServiceClient(channel);
            ViewModel = new IrcAnimeViewModel(client, new Services.DownloadStatusService(client), new Services.DownloadService(client, settingsService));
            await Task.CompletedTask;
        }
    }
}