using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;

namespace Module.AnimeSchedule.Avalonia.ViewModels
{
    public class AnimeScheduleViewModel : ModuleViewModel
    {
        private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

        public override string Name => "Anime Schedule";

        public AvaloniaList<Schedule> Schedules { get; } = new AvaloniaList<Schedule>();

        public AnimeScheduleViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
        {
            this.client = client;
        }

        public override async Task LoadAsync()
        {
            var schedules = await client.GetSchedulesAsync(new GetSchedulesRequest());
            foreach (var schedule in schedules.Schedules)
            {
                this.Schedules.Add(new Schedule()
                {
                    Name = schedule.Name,
                });
            }
        }
    }

    public class Schedule
    {
        public string Name { get; set; }
    }
}
