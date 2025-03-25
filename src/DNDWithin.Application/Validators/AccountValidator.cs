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
        RuleFor(x => x.UserName).CustomAsync(ValidateUserName);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }

    private async Task<bool> ValidateUserName(string? userName, ValidationContext<Account> context, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            context.AddFailure("Username cannot be empty");
            return false;
        }
        
        Account? userNameExists = await _accountRepository.UserNameExistsAsync(userName, token);

        if (userNameExists is not null)
        {
            context.AddFailure("Username already in use");
            return false;
        }

        return true;
    }
}