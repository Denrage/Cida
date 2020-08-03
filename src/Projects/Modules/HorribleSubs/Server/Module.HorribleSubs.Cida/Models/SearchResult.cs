using System;
using System.Collections.Generic;
using System.Text;

namespace Module.HorribleSubs.Cida.Models
{
    public class SearchResult
    {
        public string BotName { get; set; }

        public long PackageNumber { get; set; }

        public long FileSize { get; set; }

        public string FileName { get; set; }
    }
}
