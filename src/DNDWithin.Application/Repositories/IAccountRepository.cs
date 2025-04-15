using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;

namespace DNDWithin.Application.Repositories;

public interface IAccountRepository
{
    Task<bool> CreateAsync(Account account, CancellationToken token = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default);
    Task<bool> ExistsByUsernameAsync(string userName, CancellationToken token = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<Account>> GetAllAsync(GetAllAccountsOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(string? userName, CancellationToken token = default);
    Task<Account?> GetByEmailAsync(string email, CancellationToken token = default);
    Task<Account?> GetByUsernameAsync(string userName, CancellationToken token = default);
    Task<bool> UpdateAsync(Account account, CancellationToken token = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);
    Task<bool> ActivateAsync(Account account, CancellationToken token = default);
    Task<bool> UpdateActivationAsync(Account account, CancellationToken token = default);
    Task<bool> RequestPasswordResetAsync(string email, string resetCode, CancellationToken token = default);
    Task<bool> ResetPasswordAsync(PasswordReset reset, CancellationToken token = default);
    Task<bool> LoginAsync(AccountLogin accountLogin, CancellationToken token = default);
}