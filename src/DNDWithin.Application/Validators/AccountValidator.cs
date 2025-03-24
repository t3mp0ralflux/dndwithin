using DNDWithin.Application.Models;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Validators;

public class AccountValidator : AbstractValidator<Account>
{
    private readonly IAccountRepository _accountRepository;
    
    public AccountValidator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.UserName).MustAsync(ValidateUserName);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.MobilePhone).NotEmpty();
    }

    private async Task<bool> ValidateUserName(Account account, string userName, CancellationToken token = default)
    {
        Account? userNameExists = await _accountRepository.UserNameExistsAsync(account, token);

        return userNameExists is null;
    }
}