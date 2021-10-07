using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Cida.Api.Database;

public interface IDatabaseProvider
{
    void OnConfiguring(DbContextOptionsBuilder optionsBuilder, DbConnection connection);
    
    string CreateDbSql(string dbName);
    
    string CreateUserSql(string username, string password, string dbName);

    string DatabaseType { get; }
}
