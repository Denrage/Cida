using Cida.Api;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Cida.DatabaseProvider.Mssql;

public class MssqlDatabaseProvider : IDatabaseProvider
{
    public const string DatabaseTypeString = "Mssql";

    public string DatabaseType => DatabaseTypeString;

    public string CreateDbSql(string dbName)
        => $@"CREATE DATABASE {dbName};";

    public string CreateUserSql(string username, string password, string dbName)
        => $@" USE [master];
               CREATE LOGIN [{username}] WITH PASSWORD=N'{password}', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
               USE [{dbName}];
               CREATE USER [{username}] FOR LOGIN [{username}];
               ALTER ROLE [db_owner] ADD MEMBER [{username}];";

    public DbConnection GetDbConnection(string connectionString)
        => new SqlConnection(connectionString);

    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder, DbConnection connection)
        => optionsBuilder.UseSqlServer(connection);
    

    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder, string connectionString)
        => optionsBuilder.UseSqlServer(connectionString);
    
}
