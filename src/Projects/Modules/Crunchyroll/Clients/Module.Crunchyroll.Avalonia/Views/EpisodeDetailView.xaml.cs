using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.Crunchyroll.Avalonia.Views
{
    public class EpisodeDetailView : UserControl
    {
        public EpisodeDetailView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
