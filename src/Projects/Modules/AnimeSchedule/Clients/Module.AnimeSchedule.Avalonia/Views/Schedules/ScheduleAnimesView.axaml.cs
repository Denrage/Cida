using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Module.AnimeSchedule.Avalonia.Views.Schedules
{
    public partial class ScheduleAnimesView : UserControl
    {
        public ScheduleAnimesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
