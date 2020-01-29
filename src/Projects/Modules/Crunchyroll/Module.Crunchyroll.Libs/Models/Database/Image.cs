using System;

namespace Module.Crunchyroll.Libs.Models.Database
{
    public class Image
    {
        public string Thumbnail { get; set; }
        public string Small { get; set; }
        public string Medium { get; set; }
        public string Large { get; set; }
        public string Full{ get; set; }
        public string Wide { get; set; }
        public string WideWithStar { get; set; }
        public string FullWide { get; set; }
        public string FullWideWithStar { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
     
        public Guid Id { get; set; }
    }
}