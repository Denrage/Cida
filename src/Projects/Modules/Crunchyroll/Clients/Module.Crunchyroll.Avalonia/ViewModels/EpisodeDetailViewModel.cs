using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Cida.Client.Avalonia.Api;
using Crunchyroll;
using Module.Crunchyroll.Avalonia.Services;
using ReactiveUI;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class EpisodeDetailViewModel : ViewModelBase
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;
        private readonly IImageDownloadService imageDownloadService;
        private readonly EpisodeResponse.Types.EpisodeItem model;
        private IBitmap image;

        public string Id
        {
            get => this.model.Id;
            set => this.model.Id = value;
        }

        public string EpisodeNumber
        {
            get => this.model.EpisodeNumber;
            set => this.model.EpisodeNumber = value;
        }

        public string Name
        {
            get => this.model.Name;
            set => this.model.Name = value;
        }

        public string Description
        {
            get => this.model.Description;
            set => this.model.Description = value;
        }

        public IBitmap Image
        {
            get => this.image;
            private set => this.RaiseAndSetIfChanged(ref this.image, value);
        }

        public EpisodeDetailViewModel(CrunchyrollService.CrunchyrollServiceClient client,
            IImageDownloadService imageDownloadService, EpisodeResponse.Types.EpisodeItem model)
        {
            this.client = client;
            this.imageDownloadService = imageDownloadService;
            this.model = model;

            this.imageDownloadService.DownloadImageAsync(
                this.model.Image?.Thumbnail ??
                Module.ImageUnavailable,
                bitmap => this.Image = bitmap);
        }

        public async Task OpenPlayer()
        {
            var episodeUrl = await this.client.GetEpisodeStreamAsync(new EpisodeStreamRequest()
            {
                Id = this.Id,
            });

            var processStartInfo = new ProcessStartInfo(@"F:\MPV\Baka MPlayer.exe");
            processStartInfo.Arguments = "\"" + episodeUrl.StreamUrl + "\"";
            Process.Start(processStartInfo);
        }
    }
}