using System;
using Cida.Client.Avalonia.Api;
using Cida.Client.Avalonia.Services;
using ReactiveUI;

namespace Cida.Client.Avalonia.ViewModels.Launcher
{
    public class LauncherWindowViewModel : ViewModelBase
    {
        private readonly CidaConnectionService connectionService;
        private ViewModelBase content;
        private ConnectionScreenViewModel connection;
        private StatusScreenViewModel status;
        private ModuleSelectScreenViewModel moduleSelection;

        public event Action ConnectionSuccessfull;
        public event Action<ModuleViewModel> ModuleSelected;

        public ViewModelBase Content
        {
            get => this.content;
            set => this.RaiseAndSetIfChanged(ref this.content, value);
        }

        public LauncherWindowViewModel(CidaConnectionService connectionService, ISettingsFactory settingsFactory)
        {
            this.connectionService = connectionService;
            this.connection = new ConnectionScreenViewModel();
            this.status = new StatusScreenViewModel();
            this.moduleSelection = new ModuleSelectScreenViewModel(this.connectionService, settingsFactory);
            this.Content = this.connection;
            this.ConnectionSuccessfull += LauncherWindowViewModel_ConnectionSuccessfull;
        }

        ~LauncherWindowViewModel()
        {
            this.ConnectionSuccessfull -= LauncherWindowViewModel_ConnectionSuccessfull;
        }

        private async void LauncherWindowViewModel_ConnectionSuccessfull()
        {
            await this.moduleSelection.Load();
            this.Content = this.moduleSelection;
        }

        public async void Connect()
        {
            if (await this.connectionService.Connect(this.connection.Address, 31564))
            {
                this.ConnectionSuccessfull?.Invoke();
            }


        }

        public void SelectModule()
        {
            this.ModuleSelected?.Invoke(this.moduleSelection.SelectedViewModel);
        }
    }
}