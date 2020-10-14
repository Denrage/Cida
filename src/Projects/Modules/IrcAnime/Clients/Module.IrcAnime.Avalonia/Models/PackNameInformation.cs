namespace Module.IrcAnime.Avalonia.Models
{
    public class PackNameInformation
    {
        public string Name { get; set; } = null;

        public string OriginalName { get; set; } = null;

        public string Group { get; set; } = null;

        //TODO: Make this to an enum
        public string Resolution { get; set; } = null;

        public string FileExtension { get; set; } = null;

        public string EpisodeNumber { get; set; } = null;

        //TODO: Add Version bc now only the first processed version will be ever used

        //TODO: Add Identifier: One Regex to determine which groups a compound of files (name+episodenumber) so multiple resolutions will be contained but two different versions are still separated


    }
}
