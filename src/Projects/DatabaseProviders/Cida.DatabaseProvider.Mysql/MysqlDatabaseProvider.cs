using Cida.Api;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data.Common;

namespace Cida.DatabaseProvider.Mysql;

public class MysqlDatabaseProvider : IDatabaseProvider
{
    public const string DatabaseTypeString = "Mysql";

    public string DatabaseType => DatabaseTypeString;

    public string CreateDbSql(string dbName)
        => $@"CREATE DATABASE {dbName};";

    public string CreateUserSql(string username, string password, string dbName)
        => $@" CREATE USER `{username}` IDENTIFIED BY '{password}';
               GRANT USAGE ON `{dbName}`.* TO `{username}`;
               GRANT ALL privileges ON `{dbName}`.* TO `{username}`;";

    public DbConnection GetDbConnection(string connectionString)
        => new MySqlConnection(connectionString);

    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder, DbConnection connection)
        => optionsBuilder.UseMySql(connection, ServerVersion.AutoDetect(connection.ConnectionString));


    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder, string connectionString)
        => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}
