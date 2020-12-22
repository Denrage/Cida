using System;
using System.Collections.Generic;
using System.Text;

namespace Module.IrcAnime.Cida.Models.Database
{
    public class Download
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }
        
        public string Sha256 { get; set; }
        
        public ulong Size { get; set; }
        
        public string FtpPath { get; set; }
        
        public DownloadStatus DownloadStatus { get; set; }
    }

    public enum DownloadStatus
    {
        Available,
        Downloading,
        Cancelled,
        Invalid
    }
}
