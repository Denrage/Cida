using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.Crunchyroll.Avalonia.Views
{
    public class SeriesDetailView : UserControl
    {
        public SeriesDetailView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
