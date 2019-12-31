using System;
using System.Reflection;
using System.Threading.Tasks;
using Cida.Client.Avalonia.Api;
using Crunchyroll;

namespace Module.Crunchyroll.Avalonia.ViewModels
{
    public class CrunchyrollViewModel : ModuleViewModel
    {
        private readonly CrunchyrollService.CrunchyrollServiceClient client;

        public string HelloWorld => "HelloWorld";

        public CrunchyrollViewModel(CrunchyrollService.CrunchyrollServiceClient client)
        {
            this.client = client;
        }

        public override async Task LoadAsync()
        {
        }

        public override string Name => "Crunchyroll";
    }
}
