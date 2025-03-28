using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IValidator<Account> _accountValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<GetAllAccountsOptions> _optionsValidator;
    private readonly IPasswordHasher _passwordHasher;

    public AccountService(IAccountRepository accountRepository, IValidator<Account> accountValidator, IDateTimeProvider dateTimeProvider, IValidator<GetAllAccountsOptions> optionsValidator, IPasswordHasher passwordHasher)
    {
        _accountRepository = accountRepository;
        _accountValidator = accountValidator;
        _dateTimeProvider = dateTimeProvider;
        _optionsValidator = optionsValidator;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token = default)
    {
        await _accountValidator.ValidateAndThrowAsync(account, token);
        account.CreatedUtc = _dateTimeProvider.GetUtcNow();
        account.UpdatedUtc = _dateTimeProvider.GetUtcNow();
        account.Password = _passwordHasher.Hash(account.Password!);

        return await _accountRepository.CreateAsync(account, token);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _accountRepository.GetByIdAsync(id, token);
    }

    public async Task<IEnumerable<Account>> GetAllAsync(GetAllAccountsOptions options, CancellationToken token = default)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, token);

        return await _accountRepository.GetAllAsync(options, token);
    }

    public async Task<int> GetCountAsync(string? userName, CancellationToken token = default)
    {
        return await _accountRepository.GetCountAsync(userName, token);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken token = default)
    {
        return await _accountRepository.GetByEmailAsync(email, token);
    }

    public async Task<Account?> GetByUsernameAsync(string userName, CancellationToken token = default)
    {
        return await _accountRepository.GetByUsernameAsync(userName, token);
    }

    public async Task<Account?> UpdateAsync(Account account, CancellationToken token = default)
    {
        Account? existingAccount = await _accountRepository.ExistsByIdAsync(account.Id, token);
        if (existingAccount is null)
        {
            return null;
        }

        await _accountRepository.UpdateAsync(account, token);
        return account;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        return await _accountRepository.DeleteAsync(id, token);
    }
}