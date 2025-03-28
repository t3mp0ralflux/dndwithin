using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Validators.Accounts;

public class AccountValidator : AbstractValidator<Account>
{
    private readonly IAccountRepository _accountRepository;
    
    public AccountValidator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Username).CustomAsync(ValidateUserName);
        RuleFor(x => x.Email).CustomAsync(ValidateEmail).EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }

    private async Task<bool> ValidateUserName(string? userName, ValidationContext<Account> context, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            context.AddFailure("Username cannot be empty");
            return false;
        }
        
        Account? userNameExists = await _accountRepository.ExistsByUsernameAsync(userName, token);

        if (userNameExists is not null)
        {
            context.AddFailure("Username already in use");
            return false;
        }
        
        return true;
    }

    private async Task<bool> ValidateEmail(string? email, ValidationContext<Account> context, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            context.AddFailure("Email cannot be empty");
            return false;
        }

        Account? emailExists = await _accountRepository.ExistsByEmailAsync(email, token);

        if (emailExists is not null)
        {
            context.AddFailure("Email already exists. Please login instead");
            return false;
        }

        return true;
    }
}