using Cida.Client.Avalonia.Api;

namespace Cida.Client.Avalonia.ViewModels.Launcher
{
    public class StatusScreenViewModel : ViewModelBase
    {
        public string Status { get; set; }

        public StatusScreenViewModel()
        {
            this.Status = "Initializing connection ...";
        }
    }
}