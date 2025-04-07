using System.Data;
using Npgsql;

namespace DNDWithin.Application.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default);
}

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default)
    {
        NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync(token);
        return connection;
    }
}