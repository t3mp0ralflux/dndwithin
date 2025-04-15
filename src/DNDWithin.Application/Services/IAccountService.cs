using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;

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
    Task<int> GetCountAsync(string? username, CancellationToken token = default);
    Task<Account?> GetByEmailAsync(string? email, CancellationToken token = default);
    Task<Account?> GetByUsernameAsync(string username, CancellationToken token = default);
    Task<Account?> UpdateAsync(Account account, CancellationToken token = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);
    Task<bool> ActivateAsync(AccountActivation activation, CancellationToken token = default);
    Task<bool> ResendActivationAsync(AccountActivation activationRequest, CancellationToken token = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default);
    Task<bool> RequestPasswordReset(string email, CancellationToken token = default);
    Task<bool> ResetPassword(PasswordReset reset, CancellationToken token = default);
}