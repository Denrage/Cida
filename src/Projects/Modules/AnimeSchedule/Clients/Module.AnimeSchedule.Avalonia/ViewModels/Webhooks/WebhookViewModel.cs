using Animeschedule;
using Avalonia.Collections;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Webhooks;

public class WebhookViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

    private Webhook selectedWebhook;
    private ViewModelBase subViewModel;

    public AvaloniaList<Webhook> Webhooks { get; } = new();

    public Webhook SelectedWebhook
    {
        get => this.selectedWebhook;
        set
        {
            if (this.selectedWebhook != value)
            {
                this.selectedWebhook = value;
                this.RaisePropertyChanged();
                Task.Run(async () =>
                {
                    if (this.selectedWebhook != null)
                    {
                        this.selectedWebhook.Schedules = (await LoadSchedules(this.selectedWebhook.Id)).ToList();
                        SubViewModel = new WebhookDetailViewModel(client, this.selectedWebhook, this.OnChange);
                    }
                    else
                    {
                        SubViewModel = null;
                    }
                });
            }
        }
    }

    public ViewModelBase SubViewModel
    {
        get => subViewModel;
        set => this.RaiseAndSetIfChanged(ref subViewModel, value);
    }

    public WebhookViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public void Create()
    {
        SubViewModel = new WebhookDetailViewModel(client, new Webhook(), this.OnChange);
    }

    private async Task<IEnumerable<Schedule>> LoadSchedules(ulong webhookId)
    {
        var schedules = await client.GetSchedulesByWebhookAsync(new GetSchedulesByWebhookRequest()
        {
            WebhookId = webhookId,
        });

        return schedules.Schedules.Select(x => new Schedule()
        {
            Name = x.Name,
            ScheduleId = x.ScheduleId,
            // TODO: Add rest
        });
    }

    private async void OnChange(Webhook webhook)
    {
        await LoadAsync();
        if (webhook.Id != default)
        {
            this.SelectedWebhook = this.Webhooks.FirstOrDefault(x => x.Id == webhook.Id);
        }
        else
        {
            this.SelectedWebhook = null;
        }
    }

    public async Task LoadAsync()
    {
        this.Webhooks.Clear();

        var result = await client.GetWebhooksAsync(new GetWebhooksRequest());
        this.Webhooks.AddRange(result.Webhooks.Select(x => new Webhook()
        {
            Id = x.WebhookId,
            Token = x.WebhookToken
        }));
    }
}
