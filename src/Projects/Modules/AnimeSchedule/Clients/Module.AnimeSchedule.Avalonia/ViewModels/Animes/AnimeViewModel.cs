using Animeschedule;
using Avalonia.Collections;
using Cida.Client.Avalonia.Api;
using Module.AnimeSchedule.Avalonia.Extensions;
using Module.AnimeSchedule.Avalonia.Models;
using ReactiveUI;

namespace Module.AnimeSchedule.Avalonia.ViewModels.Animes;

public class AnimeViewModel : ViewModelBase
{
    private readonly AnimeScheduleService.AnimeScheduleServiceClient client;

    private AnimeInfo selectedAnime;
    private ViewModelBase subViewModel;

    public AvaloniaList<AnimeInfo> Animes { get; } = new();

    public AnimeInfo SelectedAnime
    {
        get => selectedAnime;
        set
        {
            if (selectedAnime != value)
            {
                selectedAnime = value;
                this.RaisePropertyChanged();
                Task.Run(() =>
                {
                    if (selectedAnime != null)
                    {
                        SubViewModel = new AnimeDetailViewModel(client, selectedAnime);
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

    public AnimeViewModel(AnimeScheduleService.AnimeScheduleServiceClient client)
    {
        this.client = client;
    }

    public void Create()
    {
        var editViewModel = new AnimeDetailViewModel(client, new AnimeInfo());
        SubViewModel = editViewModel;
    }

    public async Task LoadAsync()
    {
        Animes.Clear();

        var result = await client.GetAnimesAsync(new GetAnimesRequest());
        Animes.AddRange(result.Animes.Select(x => new AnimeInfo()
        {
            Id = x.Id,
            Identifier = x.Identifier,
            Type = x.Type.FromGrpc(),
            AnimeFolder = x.Folder,
            Filter = x.Filter,
        }));
    }
}
