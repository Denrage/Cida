using System;
using System.Collections.Generic;

namespace Module.Crunchyroll.Libs.Models.Database
{
    public class Collection
    {
        public string Id { get; set; }

        public Anime Anime { get; set; }

        public string AnimeId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public string Season { get; set; }

        public bool Complete { get; set; }

        public Image Landscape { get; set; }

        public Guid? LandscapeId { get; set; }
        
        public Image Portrait { get; set; }

        public Guid? PortraitId { get; set; }
        
        public string AvailabilityNotes { get; set; }

        public string Created { get; set; }   
        
        public ICollection<Episode> Episodes { get; set; }
    }
}