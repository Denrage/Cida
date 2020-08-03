using Cida.Client.Avalonia.Api;
using Horriblesubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Module.HorribleSubs.Avalonia.ViewModels
{
    public class HorribleSubsViewModel : ModuleViewModel
    {
        private readonly HorribleSubsService.HorribleSubsServiceClient client;

        public HorribleSubsViewModel(HorribleSubsService.HorribleSubsServiceClient client)
        {
            this.client = client;
        }

        public override string Name { get; } = "Horrible Subs";

        public override Task LoadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
