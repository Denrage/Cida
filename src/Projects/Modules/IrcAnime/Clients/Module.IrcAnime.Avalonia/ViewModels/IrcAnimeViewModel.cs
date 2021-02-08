using Cida.Client.Avalonia.Api;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class IrcAnimeViewModel : ModuleViewModel
    {
        public SearchViewModel Search { get; }
        
        public DownloadsViewModel Downloads { get; }

        public SettingsViewModel Settings { get; }

        public SchedulingViewModel Scheduling { get; }

        public override string Name { get; } = "IRC Anime";
        
        public IrcAnimeViewModel(SearchViewModel search, DownloadsViewModel downloads, SettingsViewModel settings, SchedulingViewModel scheduling)
        {
            this.Search = search;
            this.Downloads = downloads;
            this.Settings = settings;
            this.Scheduling = scheduling;
        }

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }
    }
}
