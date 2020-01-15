using System;

namespace Module.Crunchyroll.Libs.Models.Database
{
    public class Episode
    {
        public string Id { get; set; }

        public Collection Collection { get; set; }

        public string CollectionId { get; set; }

        public Image Image { get; set; }
        public Guid? ImageId { get; set; }

        public string EpisodeNumber { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public bool Available { get; set; }

        public bool PremiumAvailable { get; set; }

        public bool FreeAvailable { get; set; }

        public string AvailabilityNotes { get; set; }
    }
}