using System.Data;
using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Services.Implementation;

namespace DNDWithin.Application.Repositories.Implementation;

public class AccountRepository : IAccountRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDbConnectionFactory _dbConnection;

    private readonly string AccountFields = """
                                            acct.id, 
                                            acct.first_name as firstname, 
                                            acct.last_name as lastname, 
                                            acct.username, 
                                            acct.email, 
                                            acct.password, 
                                            acct.created_utc as createdutc, 
                                            acct.updated_utc as updatedutc, 
                                            acct.last_login_utc as lastloginutc, 
                                            acct.deleted_utc as deletedutc, 
                                            acct.account_status as accountstatus, 
                                            acct.account_role as accountrole,
                                            acct.activation_expiration as activationexpiration,
                                            acct.activation_code as activationcode,
                                            acct.password_reset_requested_utc as PasswordResetRequestedUtc,
                                            acct.password_reset_code as ResetCode
                                            """;

    public AccountRepository(IDbConnectionFactory dbConnection, IDateTimeProvider dateTimeProvider)
    {
        _dbConnection = dbConnection;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into account(id, first_name, last_name, username, email, password, created_utc, updated_utc, last_login_utc, deleted_utc, account_status, account_role, activation_expiration, activation_code)
                                                                         values (@Id, @FirstName, @LastName, @UserName, @Email, @Password, @CreatedUtc, @UpdatedUtc, @LastLoginUtc, @DeletedUtc, @AccountStatus, @AccountRole, @ActivationExpiration, @ActivationCode)
                                                                         on conflict do nothing
                                                                         """, account, cancellationToken: token));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        int result = await connection.QuerySingleOrDefaultAsync<int>(new CommandDefinition("""
                                                                                           select count(id) 
                                                                                           from account 
                                                                                           where id = @id 
                                                                                           """, new { id }, cancellationToken: token));
        return result > 0;
    }

    public async Task<bool> ExistsByUsernameAsync(string userName, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        int result = await connection.QuerySingleOrDefaultAsync<int>(new CommandDefinition("""
                                                                                           select count(id) 
                                                                                           from account 
                                                                                           where lower(username) = @userName 
                                                                                           """, new { userName = userName.ToLowerInvariant() }, cancellationToken: token));
        return result > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        int result = await connection.QuerySingleOrDefaultAsync<int>(new CommandDefinition("""
                                                                                           select count(id) 
                                                                                           from account 
                                                                                           where lower(email) = @email 
                                                                                           """, new { email = email.ToLowerInvariant() }, cancellationToken: token));
        return result > 0;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);

        Account? result = await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition($"""
                                                                                                      select {AccountFields}
                                                                                                      from account acct
                                                                                                      where acct.id = @id 
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
                                                                                                                        from account acct
                                                                                                                        where (@username is null or lower(username) like ('%' || @username || '%'))
                                                                                                                        and (@accountrole is null or account_role = @accountrole)
                                                                                                                        and (@accountstatus is null or account_status = @accountstatus )
                                                                                                                        {orderClause}
                                                                                                                        limit @pageSize
                                                                                                                        offset @pageOffset
                                                                                                                        """, new
                                                                                                                             {
                                                                                                                                 username = options.UserName?.ToLowerInvariant(),
                                                                                                                                 accountrole = options.AccountRole,
                                                                                                                                 accountstatus = options.AccountStatus,
                                                                                                                                 pageSize = options.PageSize,
                                                                                                                                 pageOffset = (options.Page - 1) * options.PageSize
                                                                                                                             }, cancellationToken: token));

        return results;
    }

    public async Task<int> GetCountAsync(string? username, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        return await connection.QuerySingleAsync<int>(new CommandDefinition("""
                                                                            select count(id)
                                                                            from account
                                                                            where (@userName is null || lower(username) like ('%' || @userName || '%'))
                                                                            """, new { userName = username?.ToLowerInvariant() }, cancellationToken: token));
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition($"""
                                                                                                 select {AccountFields}
                                                                                                 from account acct
                                                                                                 where lower(email) = @Email
                                                                                                 """, new { Email = email.ToLowerInvariant() }, cancellationToken: token));
    }

    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<Account>(new CommandDefinition($"""
                                                                                                 select {AccountFields}
                                                                                                 from account acct
                                                                                                 where lower(username) = @userName
                                                                                                 """, new { userName = username.ToLowerInvariant() }, cancellationToken: token));
    }

    public async Task<bool> UpdateAsync(Account account, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();
        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set first_name = @FirstName, last_name = @LastName, account_status = @AccountStatus, account_role = @AccountRole, updated_utc = @UpdatedUtc
                                                                         where id = @Id
                                                                         """, new
                                                                              {
                                                                                  account.Id,
                                                                                  account.FirstName,
                                                                                  account.LastName,
                                                                                  account.AccountStatus,
                                                                                  account.AccountRole,
                                                                                  UpdatedUtc = _dateTimeProvider.GetUtcNow()
                                                                              }, cancellationToken: token));
        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();
        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set deleted_utc = @DeletedUtc
                                                                         where id = @id
                                                                         """, new { id, DeletedUtc = _dateTimeProvider.GetUtcNow() }, cancellationToken: token));
        
        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ActivateAsync(Account account, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set account_status = @Status, activated_utc = @Activated, updated_utc = @Updated, activation_expiration = null, activation_code = null
                                                                         where id = @Id
                                                                         """, new { account.Id, Status = account.AccountStatus, Activated = account.ActivatedUtc, Updated = account.UpdatedUtc }));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> UpdateActivationAsync(Account account, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set activation_code = @ActivationCode, activation_expiration = @ActivationExpiration
                                                                         where id = @Id
                                                                         """, new { account.Id, account.ActivationCode, account.ActivationExpiration }));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> RequestPasswordResetAsync(string email, string resetCode, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set password_reset_requested_utc = @ResetUtc, password_reset_code = @ResetCode
                                                                         where lower(email) = @Email
                                                                         """, new
                                                                              {
                                                                                  ResetUtc = _dateTimeProvider.GetUtcNow(),
                                                                                  ResetCode = resetCode,
                                                                                  Email = email.ToLowerInvariant(),
                                                                                  Now = _dateTimeProvider.GetUtcNow()
                                                                              }, cancellationToken: token));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ResetPasswordAsync(PasswordReset reset, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set password = @Password, updated_utc = @Now, password_reset_requested_utc = null, password_reset_code = null
                                                                         where lower(email) = @Email
                                                                         """, new
                                                                              {
                                                                                  reset.Password,
                                                                                  Now = _dateTimeProvider.GetUtcNow(),
                                                                                  Email = reset.Email.ToLowerInvariant()
                                                                              }, cancellationToken: token));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> LoginAsync(AccountLogin accountLogin, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnection.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update account
                                                                         set password_reset_requested_utc = null, password_reset_code = null, last_login_utc = @Now
                                                                         where lower(email) = @Email
                                                                         """, new
                                                                              {
                                                                                  Now = _dateTimeProvider.GetUtcNow(),
                                                                                  Email = accountLogin.Email
                                                                              }, cancellationToken: token));
        
        transaction.Commit();
        return result > 0;
    }
}