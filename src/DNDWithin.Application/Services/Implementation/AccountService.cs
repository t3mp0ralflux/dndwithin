using System.Data;
using DNDWithin.Application.Models;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IValidator<Account> _accountValidator;
    private readonly IValidator<GetAllAccountsOptions> _optionsValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public AccountService(IAccountRepository accountRepository, IValidator<Account> accountValidator, IDateTimeProvider dateTimeProvider, IValidator<GetAllAccountsOptions> optionsValidator)
    {
        _accountRepository = accountRepository;
        _accountValidator = accountValidator;
        _dateTimeProvider = dateTimeProvider;
        _optionsValidator = optionsValidator;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token)
    {
        await _accountValidator.ValidateAndThrowAsync(account, token);
        account.CreatedUtc = _dateTimeProvider.GetUtcNow();
        account.UpdatedUtc = _dateTimeProvider.GetUtcNow();
        
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
}