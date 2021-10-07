using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Cida.Api;

public interface IDatabaseProvider
{
    void OnConfiguring(DbContextOptionsBuilder optionsBuilder, DbConnection connection);

    void OnConfiguring(DbContextOptionsBuilder optionsBuilder, string connectionString);

    string CreateDbSql(string dbName);

    string CreateUserSql(string username, string password, string dbName);

    DbConnection GetDbConnection(string connectionString);

    string DatabaseType { get; }
}
