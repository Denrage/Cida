using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class EpisodeDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        public string Id { get; set; }
        public string EpisodeNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IBitmap Image { get; set; }

        public string ImageUrl
        {
            set => Task.Run(async () =>
            {
                this.Image = await CrunchyrollViewModel.DownloadImageAsync(value);
                this.RaisePropertyChanged(nameof(Image));
            });
        }
        
        public Dictionary<string, string> Languages { get; } = new Dictionary<string, string>()
        {
            { "enUS", "Englisch" },
            { "arME", "Arabisch" },
            { "itIT", "Italienisch" },
            { "esES", "Spanisch" },
            { "frFR", "Französisch" },
            { "ptBR", "Portugiesisch" },
            { "esLA", "Spanisch (Amerikanisch)" },
            { "ruRU", "Russisch" },
            { "deDE", "Deutsch" },
        };

        public EpisodeDetailViewModel(CrunchyrollService.CrunchyrollServiceClient client)
        {
            this.client = client;
        }

        public async Task OpenPlayer()
        {
            var episodeUrl = await this.client.GetEpisodeStreamAsync(new EpisodeStreamRequest()
            {
                Id = this.Id,
            });

            var processStartInfo = new ProcessStartInfo(@"F:\MPV\Baka MPlayer.exe");
            processStartInfo.Arguments = "\"" +  episodeUrl.StreamUrl + "\""; 
            Process.Start(processStartInfo);
        }
    }
}

