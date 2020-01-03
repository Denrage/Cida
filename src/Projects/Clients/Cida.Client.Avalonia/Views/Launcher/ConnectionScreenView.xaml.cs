using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cida.Client.Avalonia.Views.Launcher
{
    public class ConnectionScreenView : UserControl
    {
        public ConnectionScreenView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
