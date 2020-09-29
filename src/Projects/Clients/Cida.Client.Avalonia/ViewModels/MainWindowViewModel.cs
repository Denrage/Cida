using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using Cida.Client.Avalonia.Api;
using Cida.Client.Avalonia.Module;
using Cida.Client.Avalonia.Services;
using ReactiveUI;

namespace Cida.Client.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ModuleViewModel Content { get; set; }
        public MainWindowViewModel(ModuleViewModel module)
        {
            Task.Run(async () => await module.LoadAsync());
            this.Content = module;
        }
    }
}
