using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Module.Crunchyroll.Libs.Models.Database
{
    public class Anime
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public Image Portrait { get; set; }
        public Guid? PortraitId { get; set; }
        
        public Image Landscape { get; set; }
        public Guid? LandscapeId { get; set; }
        
        public ICollection<Collection> Collections { get; set; }
    }
}