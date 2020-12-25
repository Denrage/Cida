using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Ircanime;
using Grpc.Core;
using Module.IrcAnime.Avalonia.ViewModels;
using Module.IrcAnime.Avalonia.Services;

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
            client = new IrcAnimeService.IrcAnimeServiceClient(channel);
            var settingsService = settingsFactory.Get(this.Name);
            var packService = new PackService();
            var downloadService = new DownloadService(client, settingsService);
            var downloadStatusService = new DownloadStatusService(client);
            var downloadContextService = new DownloadContextService(packService, downloadStatusService, settingsService, downloadService);
            
            ViewModel = new IrcAnimeViewModel(new SearchViewModel(client, downloadContextService, downloadService), new DownloadsViewModel(client, downloadContextService));
            await Task.CompletedTask;
        }
    }
}