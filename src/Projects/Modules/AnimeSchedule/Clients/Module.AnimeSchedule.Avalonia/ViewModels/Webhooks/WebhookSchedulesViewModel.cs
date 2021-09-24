using Animeschedule;
using Avalonia.Collections;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

public class ScheduleComparer : IEqualityComparer<Schedule>
{
    public bool Equals(Schedule x, Schedule y)
        => x.ScheduleId.Equals(y.ScheduleId);

    public int GetHashCode([DisallowNull] Schedule obj)
        => obj.ScheduleId.GetHashCode();
}

public class WebhookSchedulesViewModel : AssignmentViewModel<Webhook, Schedule, ScheduleComparer>
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

    public WebhookSchedulesViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Webhook item)
         : base(item)
    {
        this.client = client;
    }

    protected override void Add(Schedule item) => this.Item.Schedules.Add(item);

    protected override async Task<bool> AssignInternal(Schedule item)
    {
        var assignResult = await this.client.AssignWebhookToScheduleAsync(new AssignWebhookToScheduleRequest()
        {
            ScheduleId = item.ScheduleId,
            WebhookId = this.Item.Id,
        });

        return assignResult.AssignResult == AssignWebhookToScheduleResponse.Types.Result.Success;
    }

    protected override async Task<IEnumerable<Schedule>> GetAssignableItems()
    {
        var schedules = await this.client.GetSchedulesAsync(new GetSchedulesRequest());

        return schedules.Schedules.Select(x => new Schedule()
        {
            ScheduleId = x.ScheduleId,
            Name = x.Name,
        });
    }

    protected override IEnumerable<Schedule> GetAssignedItems()
        => this.Item.Schedules;

    protected override void Remove(Schedule item)
        => this.Item.Schedules.Remove(item);

    protected override async Task<bool> UnassignInternal(Schedule item)
    {
        var unassignResult = await client.UnassignWebhookToScheduleAsync(new UnassignWebhookToScheduleRequest()
        {
            ScheduleId = item.ScheduleId,
            WebhookId = this.Item.Id,
        });

        return unassignResult.AssignResult == UnassignWebhookToScheduleResponse.Types.Result.Success;
    }
}
