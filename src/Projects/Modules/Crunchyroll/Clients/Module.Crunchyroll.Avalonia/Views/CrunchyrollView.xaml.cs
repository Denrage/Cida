using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.Crunchyroll.Avalonia.Views
{
    public class CrunchyrollView : UserControl
    {
        public CrunchyrollView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
