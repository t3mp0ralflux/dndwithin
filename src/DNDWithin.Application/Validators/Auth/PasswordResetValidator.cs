using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Validators.Auth;

public class PasswordResetValidator : AbstractValidator<PasswordReset>
{
    private readonly IAccountRepository _accountRepository;

    public PasswordResetValidator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;

        RuleFor(x => x.Email).CustomAsync(ValidateEmail);
        RuleFor(x => x.Password).NotNull().NotEmpty();
        RuleFor(x => x.ResetCode).CustomAsync(ValidateResetCode);
    }

    private async Task<bool> ValidateEmail(string? email, ValidationContext<PasswordReset> context, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            context.AddFailure(nameof(PasswordReset.Email), "Email cannot be empty");
            return false;
        }

        bool exists = await _accountRepository.ExistsByEmailAsync(email, token);

        if (!exists)
        {
            context.AddFailure(nameof(PasswordReset.Email), "Account not found");
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateResetCode(string? resetCode, ValidationContext<PasswordReset> context, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(resetCode))
        {
            context.AddFailure(nameof(PasswordReset.ResetCode), "Reset code cannot be empty");
            return false;
        }

        Account? account = await _accountRepository.GetByEmailAsync(context.InstanceToValidate.Email, token);

        if (string.IsNullOrWhiteSpace(account!.ResetCode)) // these are checked in order and email goes first
        {
            context.AddFailure(nameof(PasswordReset.ResetCode), "Password reset not set for this account");
            return false;
        }

        if (!string.Equals(account.ResetCode, resetCode))
        {
            context.AddFailure(nameof(resetCode), "Reset code has expired or was entered incorrectly");
            return false;
        }

        return true;
    }
}