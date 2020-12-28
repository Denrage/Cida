using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Cida.Client.Avalonia.Api;
using Module.IrcAnime.Avalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aval = Avalonia;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IModuleSettingsService moduleSettingsService;
        private DownloadService.DownloadSettings downloadSettings;

        public string DownloadFolder
        {
            get => this.downloadSettings?.DownloadFolder ?? string.Empty;
            set
            {
                if (this.downloadSettings != null)
                {
                    this.downloadSettings.DownloadFolder = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public SettingsViewModel(IModuleSettingsService moduleSettingsService)
        {
            this.moduleSettingsService = moduleSettingsService;
            Task.Run(async () =>
            {
                this.downloadSettings = await this.moduleSettingsService.Get<DownloadService.DownloadSettings>();
                this.RaisePropertyChanged(null);
            });
        }

        public async Task Save()
        {
            await this.moduleSettingsService.Save(this.downloadSettings);
        }

        public async Task ChooseDownloadFolder()
        {
            var openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Directory = this.DownloadFolder;
            openFolderDialog.Title = "Choose Download folder";
            if (Aval.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var selectedFolder = await openFolderDialog.ShowAsync(desktop.MainWindow);

                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    this.DownloadFolder = selectedFolder;
                }
            }
        }

        public async Task Discard()
        {
            this.downloadSettings = await this.moduleSettingsService.Get<DownloadService.DownloadSettings>();
            this.RaisePropertyChanged(nameof(this.DownloadFolder));
        }
    }
}
