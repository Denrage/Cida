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

        public event Action ConnectionSuccessfull;

        public ViewModelBase Content
        {
            get => this.content;
            set => this.RaiseAndSetIfChanged(ref this.content, value);
        }

        public LauncherWindowViewModel(CidaConnectionService connectionService)
        {
            this.connectionService = connectionService;
            this.connection = new ConnectionScreenViewModel();
            this.status = new StatusScreenViewModel();
            this.Content = this.connection;
        }

        public async void Connect()
        {
            if(await this.connectionService.Connect(this.connection.Address, 31564))
            {
                this.ConnectionSuccessfull?.Invoke();
            }
        }
    }
}