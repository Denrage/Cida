using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using ReactiveUI;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class SchedulingViewModel : ViewModelBase
    {
        private ScheduleContext selectedItem;

        public AvaloniaList<ScheduleContext> Schedules { get; }

        public ScheduleContext SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref this.selectedItem, value);
        }

        public SchedulingViewModel()
        {
            this.Schedules = new AvaloniaList<ScheduleContext>();
        }

        public void AddSchedule()
        {
            this.Schedules.Add(new ScheduleContext() { Name = "new schedule" });
        }
    }
}
