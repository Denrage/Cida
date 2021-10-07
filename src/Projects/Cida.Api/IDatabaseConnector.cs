namespace Cida.Api;

public interface IDatabaseConnector
{
    Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password);

    IDatabaseProvider GetDatabaseProvider();
}
