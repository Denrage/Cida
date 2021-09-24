using Animeschedule;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

public class WebhookDetailViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private readonly Webhook webhook;
    private readonly Action<Webhook> onChange;

    public WebhookEditViewModel WebhookEdit { get; set; }
    public WebhookSchedulesViewModel WebhookSchedules { get; set; }

    public WebhookDetailViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Webhook webhook, Action<Webhook> onChange)
    {
        this.WebhookEdit = new WebhookEditViewModel(webhook);
        this.WebhookSchedules = new WebhookSchedulesViewModel(client, webhook);
        this.client = client;
        this.webhook = webhook;
        this.onChange = onChange;
    }

    public async Task Save()
    {
        var existingWebhook = (await this.client.GetWebhooksAsync(new GetWebhooksRequest())).Webhooks.FirstOrDefault(x => x.WebhookId == this.webhook.Id);

        await this.client.CreateDiscordWebhookAsync(new CreateDiscordWebhookRequest()
        {
            WebhookId = this.webhook.Id,
            WebhookToken = this.webhook.Token,
        });

        this.onChange(this.webhook);
    }

}
