using Cida.Client.Avalonia.Api;
using ReactiveUI;

namespace Cida.Client.Avalonia.ViewModels.Launcher
{
    public class ConnectionScreenViewModel : ViewModelBase
    {
        private string address = "127.0.0.1";

        public string Address
        {
            get => this.address;
            set => this.RaiseAndSetIfChanged(ref this.address, value);
        }
    }
}