namespace Module.HorribleSubs.Cida.Models
{
    public class DownloadRequest
    {
        public string BotName { get; set; }

        public long PackageNumber { get; set; }

        public string FileName { get; set; }
    }
}
