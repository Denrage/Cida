using Animeschedule;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Schedules
{
    public class ScheduleDetailViewModel : ViewModelBase
    {
        private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
        private readonly Schedule schedule;
        private readonly Action<Schedule> onChange;

        public EditScheduleViewModel Edit { get; set; }
        public ScheduleAnimesViewModel Assign { get; set; }

        public ScheduleDetailViewModel(Animeschedule.AnimeScheduleService.AnimeScheduleServiceClient client, Schedule schedule, Action<Schedule> onChange)
        {
            this.Edit = new EditScheduleViewModel(schedule);
            this.Assign = new ScheduleAnimesViewModel(client, schedule);
            this.client = client;
            this.schedule = schedule;
            this.onChange = onChange;
        }

        public async Task Save()
        {
            var edit = false;
            if (schedule.ScheduleId != default)
            {
                edit = true;
            }

            var createResult = await client.CreateScheduleAsync(new CreateScheduleRequest()
            {
                Interval = schedule.Interval.ToDuration(),
                Name = schedule.Name,
                ScheduleId = schedule.ScheduleId,
                StartDate = schedule.StartDate.ToUniversalTime().ToTimestamp(),
                Override = edit,
            });

            if (createResult.CreateResult != CreateScheduleResponse.Types.Result.Success)
            {
                // TODO: Do something here
            }
            else
            {
                schedule.ScheduleId = createResult.ScheduleId;
            }

            this.onChange(this.schedule);
        }
    }
}
