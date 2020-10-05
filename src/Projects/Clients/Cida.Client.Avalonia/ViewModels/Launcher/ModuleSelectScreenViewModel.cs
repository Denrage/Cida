using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Cida.Client.Avalonia.Module;
using Cida.Client.Avalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cida.Client.Avalonia.ViewModels.Launcher
{
    public class ModuleSelectScreenViewModel : ViewModelBase
    {
        public AvaloniaList<IModule> Modules { get; } = new AvaloniaList<IModule>();

        private IModule selectedModule;
        private readonly CidaConnectionService connectionService;

        public IModule SelectedModule
        {
            get => this.selectedModule;
            set
            {
                if (this.selectedModule != value && value != null)
                {
                    this.RaiseAndSetIfChanged(ref this.selectedModule, value);
                    this.OnSelectedModuleChanged();
                }
            }
        }

        private void OnSelectedModuleChanged()
        {
            this.SelectedViewModel = this.selectedModule.ViewModel;
        }

        public ModuleViewModel SelectedViewModel { get; private set; }

        public ModuleSelectScreenViewModel(CidaConnectionService connectionService)
        {
            this.connectionService = connectionService;
        }

        public async Task Load()
        {
            //var client = new CidaApiService.CidaApiServiceClient(this.connectionService.Channel);
            //var modules = (await client.ClientModuleAsync(new ClientModuleRequest() { Id = "F38224C1-7588-4773-A345-A238B8937A8F" })).Streams.Select(x => AvaloniaModule.Extract(x.ToByteArray())).Select(x => x.Module).ToArray();

            var modules = new[] { AvaloniaModule.Unpacked(@"D:\Repos\Cida\src\Projects\Modules\IrcAnime\Clients\Module.IrcAnime.Avalonia\bin\Debug\netstandard2.1").Module, AvaloniaModule.Unpacked(@"D:\Repos\Cida\src\Projects\Modules\Crunchyroll\Clients\Module.Crunchyroll.Avalonia\bin\Debug\netstandard2.1").Module };

            foreach (var module in modules)
            {
                await module.LoadAsync(this.connectionService.Channel);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Modules.Clear();
                this.Modules.AddRange(modules);
            });
        }
    }
}
