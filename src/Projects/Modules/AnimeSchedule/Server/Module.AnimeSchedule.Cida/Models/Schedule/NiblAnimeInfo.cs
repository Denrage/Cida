using Module.AnimeSchedule.Cida.Models.Database;

namespace Module.AnimeSchedule.Cida.Models.Schedule
{
    public class NiblAnimeInfo : AnimeInfo
    {
        public ulong PackageNumber { get; set; }

        public string DestinationFolderName { get; set; }

        public string Bot { get; set; }

        public override Episode ToDb()
        {
            return new Episode()
            {
                EpisodeNumber = this.EpisodeNumber,
                Name = this.Name,
                PackageNumber = new PackageNumber()
                {
                    Name = string.Empty,
                    Number = this.PackageNumber,
                },
            };
        }
    }
}
