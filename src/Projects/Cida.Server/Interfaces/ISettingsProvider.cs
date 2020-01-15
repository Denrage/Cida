namespace Cida.Server.Interfaces
{
    public interface ISettingsProvider
    {
        void Save<T>(T settings)
            where T : class;

        T Get<T>()
            where T : class, new();
    }
}
