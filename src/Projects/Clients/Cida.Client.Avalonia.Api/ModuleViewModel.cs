using System.Threading.Tasks;

namespace Cida.Client.Avalonia.Api
{
    public abstract class ModuleViewModel : ViewModelBase
    {
        public abstract string Name { get; }

        public abstract Task LoadAsync();
    }
}