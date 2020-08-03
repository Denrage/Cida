using System;
using System.Collections.Generic;
using System.Text;

namespace Module.Crunchyroll.WebApp.Services
{
    public class AnimeSelectionEventService
    {
        public event EventHandler<Libs.Models.Search.SearchItem> AnimeSearched;

        public void FireEvent(object sender, Libs.Models.Search.SearchItem anime)
        {
            this.AnimeSearched?.Invoke(sender, anime);
        }
    }
}
