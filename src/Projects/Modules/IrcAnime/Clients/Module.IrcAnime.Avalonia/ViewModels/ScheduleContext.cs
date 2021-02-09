using System;
using System.Collections.Generic;
using System.Text;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class ScheduleContext
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        // TODO: Add converter for TimeSpan
        public string Schedule { get; set; } = default;
    }
}
