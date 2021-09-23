namespace Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

public class WebhookEditViewModel : ViewModelBase
{
    private readonly Webhook webhook;

    public Webhook Webhook => this.webhook;

    public WebhookEditViewModel(Webhook webhook)
    {
        this.webhook = webhook;
    }
}
