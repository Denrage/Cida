using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cida.Client.Avalonia.Views
{
    public class LauncherWindow : Window
    {
        public LauncherWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
