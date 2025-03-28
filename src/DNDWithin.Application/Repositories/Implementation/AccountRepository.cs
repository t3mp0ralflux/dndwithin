using System.Data;
using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;

namespace DNDWithin.Application.Repositories.Implementation;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _dbConnection;

    private readonly string AccountFields = """
                                            id, 
                                            first_name as firstname, 
                                            last_name as lastname, 
                                            username, 
                                            email, 
                                            password, 
                                            created_utc as createdutc, 
                                            updated_utc as updatedutc, 
                                            last_login_utc as lastloginutc, 
                                            deleted_utc as deletedutc, 
                                            account_status as accountstatus, 
                                            account_role as accountrole 
                                            """;

    public AccountRepository(IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into account(id, first_name, last_name, username, email, password, created_utc, updated_utc, last_login_utc, deleted_utc, account_status, account_role)
                                                                         values (@Id, @FirstName, @LastName, @UserName, @Email, @Password, @CreatedUtc, @UpdatedUtc, @LastLoginUtc, @DeletedUtc, @AccountStatus, @AccountRole)
                                                                         """, account, cancellationToken: token));

        transaction.Commit();

        return result > 0;
    }

    public async Task<Account?> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        Account? result = await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition("""
                                                                                                    select * from account where id = @id 
                                                                                                    """, new { id }, cancellationToken: token));
        return result;
    }

    public async Task<Account?> ExistsByUsernameAsync(string userName, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        Account? result = await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition("""
                                                                                                    select * from account where username = @userName 
                                                                                                    """, new { userName }, cancellationToken: token));
        return result;
    }

    public async Task<Account?> ExistsByEmailAsync(string email, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        Account? result = await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition("""
                                                                                                    select * from account where email = @email 
                                                                                                    """, new { email }, cancellationToken: token));
        return result;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        Account? result = await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition($"""
                                                                                                      select {AccountFields} from account where id = @id
                                                                                                     """, new { id }, cancellationToken: token));

        return result;
    }

    public async Task<IEnumerable<Account>> GetAllAsync(GetAllAccountsOptions options, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        string orderClause = string.Empty;

        if (options.SortField is not null)
        {
            orderClause = $"order by {options.SortField} {(options.SortOrder == SortOrder.ascending ? "asc" : "desc")}";
        }

        IEnumerable<Account> results = await connection.QueryAsync<Account>(new CommandDefinition($"""
                                                                                                   select {AccountFields}
                                                                                                   from account
                                                                                                   where (@username is null or username like ('%' || @username || '%'))
                                                                                                   and (@accountrole is null or account_role = @accountrole)
                                                                                                   and (@accountstatus is null or account_status = @accountstatus )
                                                                                                   {orderClause}
                                                                                                   limit @pageSize
                                                                                                   offset @pageOffset
                                                                                                   """, new
                                                                                                        {
                                                                                                            username = options.UserName,
                                                                                                            accountrole = options.AccountRole,
                                                                                                            accountstatus = options.AccountStatus,
                                                                                                            pageSize = options.PageSize,
                                                                                                            pageOffset = (options.Page - 1) * options.PageSize
                                                                                                        }, cancellationToken: token));

        return results;
    }

    public async Task<int> GetCountAsync(string? userName, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        return await connection.QuerySingleAsync<int>(new CommandDefinition("""
                                                                            select count(id)
                                                                            from account
                                                                            where (@userName is null || username like ('%' || @userName || '%'))
                                                                            """, new { userName }, cancellationToken: token));
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition($"""
                                                                                          select {AccountFields}
                                                                                          from account
                                                                                          where email = @email
                                                                                          """, new { email }, cancellationToken: token));
    }

    public async Task<Account?> GetByUsernameAsync(string userName, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition($"""
                                                                                          select {AccountFields}
                                                                                          from account
                                                                                          where username = @userName
                                                                                          """, new { userName }, cancellationToken: token));
    }

    public async Task<bool> UpdateAsync(Account account, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set first_name = @FirstName, last_name = @LastName, account_status = @AccountStatus, account_role = @AccountRole
                                                                         where id = @Id
                                                                         """, account, cancellationToken: token));
        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         delete from accounts
                                                                         where id = @id
                                                                         """, new { id }, cancellationToken: token));

        return result > 0;
    }
}