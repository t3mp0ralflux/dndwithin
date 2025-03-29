﻿using DNDWithin.Application.Models.Accounts;

namespace DNDWithin.Application.Repositories;

public interface IAccountRepository
{
    Task<bool> CreateAsync(Account account, CancellationToken token = default);
    Task<Account?> ExistsByIdAsync(Guid id, CancellationToken token = default);
    Task<Account?> ExistsByUsernameAsync(string userName, CancellationToken token = default);
    Task<Account?> ExistsByEmailAsync(string email, CancellationToken token = default);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<Account>> GetAllAsync(GetAllAccountsOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(string? userName, CancellationToken token = default);
    Task<Account?> GetByEmailAsync(string email, CancellationToken token = default);
    Task<Account?> GetByUsernameAsync(string userName, CancellationToken token = default);
    Task<bool> UpdateAsync(Account account, CancellationToken token = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);
}