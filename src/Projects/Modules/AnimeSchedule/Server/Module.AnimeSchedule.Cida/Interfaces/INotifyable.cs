using Discord;

namespace Module.AnimeSchedule.Cida.Interfaces;

public interface INotifyable : IActionable
{
    Task<Embed> CreateEmbed();
}
