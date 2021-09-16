using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.AnimeSchedule.Avalonia.Views
{
    public partial class EditScheduleView : UserControl
    {
        public EditScheduleView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
