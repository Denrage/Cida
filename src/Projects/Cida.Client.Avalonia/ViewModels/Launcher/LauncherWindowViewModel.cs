using ReactiveUI;

namespace Cida.Client.Avalonia.ViewModels.Launcher
{
    public class LauncherWindowViewModel : ViewModelBase
    {
        private ViewModelBase content;
        private ConnectionScreenViewModel connection;
        private StatusScreenViewModel status;
        public ViewModelBase Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public LauncherWindowViewModel()
        {
            this.connection = new ConnectionScreenViewModel();
            this.status = new StatusScreenViewModel();
            this.Content = connection;
        }

        public void Connect()
        {
            this.Content = status;
        }
    }
}