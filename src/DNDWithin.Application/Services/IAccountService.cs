using DNDWithin.Application.Models;

namespace DNDWithin.Application.Services;

public interface IAccountService
{
    /// <summary>
    /// Create a new account
    /// </summary>
    /// <param name="account">Account to be created</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    Task<bool> CreateAsync(Account account, CancellationToken token = default);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<Account>> GetAllAsync(GetAllAccountsOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(string? userName, CancellationToken token = default);
}