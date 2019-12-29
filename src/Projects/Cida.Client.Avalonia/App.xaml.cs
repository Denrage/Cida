using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cida.Client.Avalonia.ViewModels;
using Cida.Client.Avalonia.ViewModels.Launcher;
using Cida.Client.Avalonia.Views;

namespace Cida.Client.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this); 
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new LauncherWindow()
                {
                    DataContext = new LauncherWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}