using Cida.Client.Avalonia.Api;

namespace Cida.Client.Avalonia.Services
{
    internal class SettingsFactory : ISettingsFactory
    {
        private readonly ISettingsService settingsService;

        public SettingsFactory(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }
        public IModuleSettingsService Get(string moduleName)
        {
            return new ModuleSettingsService(moduleName, this.settingsService);
        }
    }
}