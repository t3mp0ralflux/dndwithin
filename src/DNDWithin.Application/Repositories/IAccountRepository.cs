using DNDWithin.Application.Models.Accounts;

namespace DNDWithin.Application.Repositories;

public interface IAccountRepository
{
    Task<bool> CreateAsync(Account account, AccountActivation activation, CancellationToken token = default);
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
    Task<bool> UpdateActivationAsync(Guid accountId, AccountActivation accountActivation, CancellationToken token = default);
}