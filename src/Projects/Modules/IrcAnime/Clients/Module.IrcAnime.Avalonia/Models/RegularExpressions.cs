using System.Collections.Generic;

namespace Module.IrcAnime.Avalonia.Models
{
    public class RegularExpressions
    {
        public string Group { get; set; }

        public string Resolution { get; set; }

        public string FileExtension { get; set; }

        public string EpisodeNumber { get; set; }

        public List<string> Remove { get; set; }
    }
}
