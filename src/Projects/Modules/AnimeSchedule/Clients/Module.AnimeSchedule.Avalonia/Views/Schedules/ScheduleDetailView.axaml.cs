using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.AnimeSchedule.Avalonia.Views.Schedules
{
    public partial class ScheduleDetailView : UserControl
    {
        public ScheduleDetailView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
