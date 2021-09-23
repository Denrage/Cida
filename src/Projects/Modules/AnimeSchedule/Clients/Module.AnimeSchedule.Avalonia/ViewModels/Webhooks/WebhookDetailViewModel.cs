using Animeschedule;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

public class WebhookDetailViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;
    private readonly Webhook webhook;

    public event Action OnSave;

    public WebhookEditViewModel WebhookEdit { get; set; }
    public WebhookSchedulesViewModel WebhookSchedules { get; set; }

    public WebhookDetailViewModel(AnimeScheduleService.AnimeScheduleServiceClient client, Webhook webhook)
    {
        this.WebhookEdit = new WebhookEditViewModel(webhook);
        this.WebhookSchedules = new WebhookSchedulesViewModel(client, webhook);
        this.client = client;
        this.webhook = webhook;
    }

    public async Task Save()
    {
        var existingWebhook = (await this.client.GetWebhooksAsync(new GetWebhooksRequest())).Webhooks.FirstOrDefault(x => x.WebhookId == this.webhook.Id);

        await this.client.CreateDiscordWebhookAsync(new CreateDiscordWebhookRequest()
        {
            WebhookId = this.webhook.Id,
            WebhookToken = this.webhook.Token,
        });

        Task.Run(() => this.OnSave?.Invoke());
    }

}
