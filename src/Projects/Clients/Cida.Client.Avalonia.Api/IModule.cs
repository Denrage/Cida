using System;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;

namespace Cida.Client.Avalonia.Api
{
    public interface IModule
    {
        string Name { get; }

        ModuleViewModel ViewModel { get; }

        Task LoadAsync(Channel channel);
    }
}
