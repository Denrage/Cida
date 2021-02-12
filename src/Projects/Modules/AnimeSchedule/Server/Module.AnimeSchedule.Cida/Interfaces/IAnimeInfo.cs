namespace Module.AnimeSchedule.Cida.Interfaces
{
    public interface IAnimeInfo
    {
        double EpisodeNumber { get; }

        string Name { get; }

        ulong MyAnimeListId { get; }
    }
}
