using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Ircanime;
using Module.IrcAnime.Avalonia.Models;
using Module.IrcAnime.Avalonia.Services;
using ReactiveUI;
using SharpDX.Direct2D1;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Module.IrcAnime.Avalonia.ViewModels
{
    public class IrcAnimeViewModel : ModuleViewModel
    {
        public SearchViewModel Search { get; }
        
        public DownloadsViewModel Downloads { get; }

        public override string Name { get; } = "IRC Anime";
        
        public IrcAnimeViewModel(SearchViewModel search, DownloadsViewModel downloads)
        {
            this.Search = search;
            this.Downloads = downloads;
        }

        public override async Task LoadAsync()
        {
            await Task.CompletedTask;
        }
    }
}
