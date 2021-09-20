using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.AnimeSchedule.Avalonia.Views
{
    public partial class AnimeView : UserControl
    {
        public AnimeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
