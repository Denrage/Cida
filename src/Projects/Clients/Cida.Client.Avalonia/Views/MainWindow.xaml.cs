using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cida.Client.Avalonia.ViewModels;

namespace Cida.Client.Avalonia.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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