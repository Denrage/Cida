using Animeschedule;
using Avalonia.Collections;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

public class WebhookSchedulesViewModel : ViewModelBase
{
    private readonly List<Schedule> allSchedules = new();
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private Webhook webhook;

    public Webhook Webhook
    {
        get => this.webhook;
        set
        {
            if (this.webhook != value)
            {
                this.webhook = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.WebhookSchedules));
                this.RaisePropertyChanged(nameof(this.NotInWebhookSchedule));
            }
        }
    }

    public AvaloniaList<Schedule> WebhookSchedules => new AvaloniaList<Schedule>(this.Webhook?.Schedules ?? Enumerable.Empty<Schedule>());

    public AvaloniaList<Schedule> NotInWebhookSchedule => new AvaloniaList<Schedule>(this.allSchedules.Where(x => !this.WebhookSchedules.Select(y => y.ScheduleId).Contains(x.ScheduleId)));

    public WebhookSchedulesViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Webhook webhook)
    {
        this.client = client;
        this.webhook = webhook;
        Task.Run(async () =>
        {
            var schedules = await this.client.GetSchedulesAsync(new GetSchedulesRequest());

            this.allSchedules.AddRange(schedules.Schedules.Select(x => new Schedule()
            {
                ScheduleId = x.ScheduleId,
                Name = x.Name,
            }));
            this.RaisePropertyChanged(nameof(this.NotInWebhookSchedule));
        });
    }

    public async Task AssignSchedule(Schedule schedule)
    {
        var assignResult = await client.AssignWebhookToScheduleAsync(new AssignWebhookToScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
            WebhookId = this.Webhook.Id,
        });

        if (assignResult.AssignResult == AssignWebhookToScheduleResponse.Types.Result.Success)
        {
            this.Webhook.Schedules.Add(schedule);
            this.RaisePropertyChanged(nameof(this.WebhookSchedules));
            this.RaisePropertyChanged(nameof(this.NotInWebhookSchedule));
        }
    }

    public async Task UnassignSchedule(Schedule schedule)
    {
        var unassignResult = await client.UnassignWebhookToScheduleAsync(new UnassignWebhookToScheduleRequest()
        {
            ScheduleId = schedule.ScheduleId,
            WebhookId = this.Webhook.Id,
        });

        if (unassignResult.AssignResult == UnassignWebhookToScheduleResponse.Types.Result.Success)
        {
            this.Webhook.Schedules.Remove(schedule);
            this.RaisePropertyChanged(nameof(this.WebhookSchedules));
            this.RaisePropertyChanged(nameof(this.NotInWebhookSchedule));
        }
    }
}
