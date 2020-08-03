using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Horriblesubs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Module.HorribleSubs.Avalonia.ViewModels
{
    public class HorribleSubsViewModel : ModuleViewModel
    {
        private readonly HorribleSubsService.HorribleSubsServiceClient client;

        private string searchTerm;


        public string SearchTerm
        {
            get => this.searchTerm;
            set
            {
                this.RaiseAndSetIfChanged(ref this.searchTerm, value);
            }
        }

        public AvaloniaList<PackItem> Packs { get; } = new AvaloniaList<PackItem>();

        public HorribleSubsViewModel(HorribleSubsService.HorribleSubsServiceClient client)
        {
            this.client = client;
        }

        public override string Name { get; } = "Horrible Subs";

        public override Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        public async Task Search()
        {
            if (!string.IsNullOrEmpty(this.SearchTerm))
            {
                var result = await this.client.SearchAsync(new SearchRequest()
                {
                    SearchTerm = this.SearchTerm
                });

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.Packs.Clear();
                    this.Packs.AddRange(result.SearchResults.Select(x => new PackItem(x.FileName, x.PackageNumber.ToString(), x.BotName)));
                });

            }
        }
    }

    public class PackItem
    {
        private static readonly Regex ResolutionRegex = new Regex("(480p|720p|1080p)");
        private static readonly Regex BracketsRegex = new Regex("\\[.*?\\]");
        private static readonly Regex BracesRegex = new Regex("\\(.*?\\)");
        public string Name { get; }

        public string PackNumber { get; }

        public string Bot { get; }

        public string Resolution { get; }

        public PackItem(string name, string packNumber, string bot)
        {
            this.Name = name;
            foreach (var match in BracesRegex.Matches(this.Name).Concat(BracketsRegex.Matches(this.Name)))
            {
                this.Name = this.Name.Replace(match.ToString(), string.Empty);
            }

            this.Name = this.Name.Trim();

            this.Resolution = ResolutionRegex.Match(name).Value;

            this.PackNumber = packNumber;
            this.Bot = bot;
        }
    }
}
