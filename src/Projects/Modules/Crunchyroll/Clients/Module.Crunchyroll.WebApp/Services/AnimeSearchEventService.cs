using System;
using System.Collections.Generic;
using System.Text;

namespace Module.Crunchyroll.WebApp.Services
{
    public class AnimeSearchEventService
    {
        public event EventHandler<IEnumerable<Libs.Models.Search.SearchItem>> AnimesSearched;

        public void FireEvent(object sender, IEnumerable<Libs.Models.Search.SearchItem> animes)
        {
            this.AnimesSearched?.Invoke(sender, animes);
        }
    }
}
