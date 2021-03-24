using System.Threading.Tasks;

namespace Cida.Client.Avalonia.Services
{
    public interface ISettingsService
    {
        Task<T> Get<T>(string moduleName)
                where T : class, new();

        Task Save<T>(string moduleName, T item)
                where T : class;
    }
}