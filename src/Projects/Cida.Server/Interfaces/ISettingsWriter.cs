namespace Cida.Server.Interfaces
{
    public interface ISettingsWriter
    {
        void Save(string settings);

        string Get();
    }
}