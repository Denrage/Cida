using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Ircanime;
using Module.IrcAnime.Avalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        private readonly IrcAnimeService.IrcAnimeServiceClient client;
        private readonly DownloadContextService downloadContextService;
        private readonly IModuleSettingsService moduleSettingsService;

        public AvaloniaList<DownloadContext> AvailableDownloads { get; } = new AvaloniaList<DownloadContext>();

        public IEnumerable<DownloadContext> FilteredDownloads => string.IsNullOrEmpty(this.Filter) ? this.AvailableDownloads : this.AvailableDownloads.Where(x => x.Pack.Name.ToLower().Contains(this.Filter));

        private string filter;

        public string Filter
        {
            get => filter;
            set 
            {
                this.RaiseAndSetIfChanged(ref this.filter, value);
                this.RaisePropertyChanged(nameof(this.FilteredDownloads));
            }
        }


        public ICommand FocusCommand { get; }

        public DownloadsViewModel(IrcAnimeService.IrcAnimeServiceClient client, DownloadContextService downloadContextService, IModuleSettingsService moduleSettingsService)
        {
            this.client = client;
            this.downloadContextService = downloadContextService;
            this.moduleSettingsService = moduleSettingsService;
            this.FocusCommand = ReactiveCommand.Create(async () => await RefreshAvailableDownloads());
        }

        public async Task RefreshAvailableDownloads()
        {
            var files = (await this.client.DownloadedFilesAsync(new DownloadedFilesRequest())).Files;
            var contexts = new List<DownloadContext>();
            foreach (var item in files)
            {
                var context = this.downloadContextService.GetContext(new Models.PackMetadata()
                {
                    Bot = null,
                    Name = item.Filename,
                    Number = 0,
                    Size = item.Filesize,
                });

                contexts.Add(context);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.AvailableDownloads.Clear();
                this.AvailableDownloads.AddRange(contexts);
            });

            //TODO: Reminder for future use
            //await this.downloadService.Download(this.AvailableDownloads[0].Name, default);
        }

        public async Task OpenDownloadsFolder()
        {
            // TODO: Only make this usable on windows or find a way for cross platform
            var settingsFolder = await this.moduleSettingsService.Get<DownloadService.DownloadSettings>();
            Process.Start("explorer.exe", settingsFolder.DownloadFolder);
        }
    }
}
