namespace Cida.Api;

public interface IDatabaseConnector
{
    event Action OnConnectionChanged;

    Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password);

    IDatabaseProvider GetDatabaseProvider();
}
