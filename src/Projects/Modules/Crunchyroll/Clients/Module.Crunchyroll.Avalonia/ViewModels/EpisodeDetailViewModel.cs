using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Crunchyroll;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class EpisodeDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        public string Id { get; set; }
        public string EpisodeNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
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

