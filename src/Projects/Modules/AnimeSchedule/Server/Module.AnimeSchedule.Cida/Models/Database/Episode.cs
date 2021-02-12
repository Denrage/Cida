namespace Module.AnimeSchedule.Cida.Models.Database
{
    public class Episode
    {
        public string Name { get; set; }

        public AnimeContext AnimeContext { get; set; }

        public double EpisodeNumber { get; set; }

        public PackageNumber PackageNumber { get; set; }
    }
}
