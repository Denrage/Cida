using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cida.Client.Avalonia.Services;
using Cida.Client.Avalonia.ViewModels;
using Cida.Client.Avalonia.ViewModels.Launcher;
using Cida.Client.Avalonia.Views;
using Cida.Client.Avalonia.Views.Launcher;

namespace Cida.Client.Avalonia
{
    public class App : Application
    {
        private readonly CidaConnectionService connectionService = new CidaConnectionService();
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this); 
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
                var viewModel = new LauncherWindowViewModel(this.connectionService);
                viewModel.ConnectionSuccessfull += () =>
                {
                    var oldWindow = desktop.MainWindow;
                    desktop.MainWindow = new MainWindow()
                    {
                        DataContext = new MainWindowViewModel(this.connectionService),
                    };
                    desktop.MainWindow.Show();
                    oldWindow.Close();
                };
                desktop.MainWindow = new LauncherWindow()
                {
                    DataContext = viewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}