using System.Data;
using Dapper;

namespace DNDWithin.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        await connection.ExecuteAsync(new CommandDefinition("""
                                                            insert into account(id, first_name, last_name, username, email, mobile_phone, created_utc, updated_utc, last_login, deleted_utc, account_status, account_role)
                                                            values('4174494b-9d60-4d11-bb4a-eff736cc5bf8', 'Brent', 'Belanger', 't3mp0ralflux', 't3mp0ralflux@gmail.com', '5713164815', '2025-03-24T09:40:00', '2025-03-24T09:40:00', '2025-03-24T09:40:00', null, 0, 0)
                                                            on conflict do nothing
                                                            """));
    }
}