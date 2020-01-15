using System;
using System.Threading.Tasks;

namespace Cida.Api
{
    public interface IDatabaseConnector
    {
        Task<string> GetDatabaseConnectionStringAsync(Guid moduleId, string password);
    }
}