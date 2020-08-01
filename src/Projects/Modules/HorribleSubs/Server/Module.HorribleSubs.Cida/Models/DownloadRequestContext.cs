using IrcClient.Downloaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.HorribleSubs.Cida.Models
{
    public class DownloadRequestContext
    {
        public DccDownloader Downloader { get; }
    
        public DownloadRequest DownloadRequest { get; }

        public DownloadRequestContext(DccDownloader downloader, DownloadRequest request)
        {
            this.DownloadRequest = request;
            this.Downloader = downloader;
        }
    }
}
