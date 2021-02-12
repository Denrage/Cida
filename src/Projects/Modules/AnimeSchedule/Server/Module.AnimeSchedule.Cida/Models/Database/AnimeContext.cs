using System.Collections.Generic;

namespace Module.AnimeSchedule.Cida.Models.Database
{
    public class AnimeContext
    {
        public ulong MyAnimeListId { get; set; }

        public string Identifier { get; set; }

        public string Filter { get; set; }

        public AnimeContextType Type { get; set; }

        public Schedule Schedule { get; set; }

        public string FolderName { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}
