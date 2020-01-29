using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.Crunchyroll.Avalonia.Views
{
    public class CollectionDetailView : UserControl
    {
        public CollectionDetailView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
