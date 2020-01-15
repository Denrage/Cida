using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Cida.Client.Avalonia.Module;
using Cida.Client.Avalonia.Services;
using ReactiveUI;

namespace Cida.Client.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly CidaConnectionService connectionService;
        private ViewModelBase content;

        public ViewModelBase Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public AvaloniaList<IModule> Modules { get; } = new AvaloniaList<IModule>();

        private IModule selectedModule;
        private ModuleViewModel selectedViewModel;

        public IModule SelectedModule
        {
            get => selectedModule;
            set
            {
                if (this.selectedModule != value && value != null)
                {
                    this.RaiseAndSetIfChanged(ref this.selectedModule, value);
                    this.OnSelectedModuleChanged();
                }
            }
        }

        public ModuleViewModel SelectedViewModel
        {
            get => this.selectedViewModel;
            set => this.RaiseAndSetIfChanged(ref this.selectedViewModel, value);
        }

        private void OnSelectedModuleChanged()
        {
            this.SelectedViewModel = this.SelectedModule.ViewModel;
            Task.Run(async () => await this.SelectedViewModel.LoadAsync());
        }

        public MainWindowViewModel(CidaConnectionService connectionService)
        {
            this.connectionService = connectionService;
            Task.Run(async () =>
            {
                //var client = new CidaApiService.CidaApiServiceClient(this.connectionService.Channel);
                //var modules = (await client.ClientModuleAsync(new ClientModuleRequest() { Id = "F38224C1-7588-4773-A345-A238B8937A8F" })).Streams.Select(x => AvaloniaModule.Extract(x.ToByteArray())).Select(x => x.Module).ToArray();

                var modules = new[] {AvaloniaModule.Unpacked(@"C:\Repos\Cida\src\Projects\Modules\Crunchyroll\Clients\Module.Crunchyroll.Avalonia\bin\Debug\netstandard2.1").Module};

                foreach (var module in modules)
                {
                    await module.LoadAsync(this.connectionService.Channel);
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.Modules.Clear();
                    this.Modules.AddRange(modules);
                });
            });
        }
    }
}
