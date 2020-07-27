using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cida.Client.Avalonia.Views.Launcher
{
    public class StatusScreenView : UserControl
    {
        public StatusScreenView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
