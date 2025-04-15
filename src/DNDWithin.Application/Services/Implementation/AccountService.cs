using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Models.System;
using DNDWithin.Application.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DNDWithin.Application.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IValidator<Account> _accountValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;
    private readonly IGlobalSettingsService _globalSettingsService;
    private readonly ILogger<AccountService> _logger;
    private readonly IValidator<GetAllAccountsOptions> _optionsValidator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<PasswordReset> _passwordResetValidator;

    public AccountService(IAccountRepository accountRepository, IValidator<Account> accountValidator, IDateTimeProvider dateTimeProvider, IValidator<GetAllAccountsOptions> optionsValidator, IPasswordHasher passwordHasher, IGlobalSettingsService globalSettingsService, IEmailService emailService, ILogger<AccountService> logger, IValidator<PasswordReset> passwordResetValidator)
    {
        _accountRepository = accountRepository;
        _accountValidator = accountValidator;
        _optionsValidator = optionsValidator;
        _passwordResetValidator = passwordResetValidator;
        _passwordHasher = passwordHasher;
        _globalSettingsService = globalSettingsService;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token = default)
    {
        await _accountValidator.ValidateAndThrowAsync(account, token);
        account.CreatedUtc = _dateTimeProvider.GetUtcNow();
        account.UpdatedUtc = _dateTimeProvider.GetUtcNow();
        account.Password = _passwordHasher.Hash(account.Password);

        await CreateActivationData(account, token);

        bool success = await _accountRepository.CreateAsync(account, token);

        if (!success)
        {
            return false;
        }

        try
        {
            await QueueActivationEmail(account, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        return true;
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

    public async Task<int> GetCountAsync(string? username, CancellationToken token = default)
    {
        return await _accountRepository.GetCountAsync(username, token);
    }

    public async Task<Account?> GetByEmailAsync(string? email, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _accountRepository.GetByEmailAsync(email, token);
    }

    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken token = default)
    {
        return await _accountRepository.GetByUsernameAsync(username, token);
    }

    public async Task<Account?> UpdateAsync(Account account, CancellationToken token = default)
    {
        bool existingAccount = await _accountRepository.ExistsByIdAsync(account.Id, token);
        if (!existingAccount)
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

    public async Task<bool> ActivateAsync(AccountActivation activation, CancellationToken token = default)
    {
        Account? existingAccount = await _accountRepository.GetByUsernameAsync(activation.Username, token);

        if (existingAccount is null)
        {
            throw new ValidationException("No account found");
        }

        if (existingAccount.ActivationCode != activation.ActivationCode || existingAccount.ActivationExpiration < _dateTimeProvider.GetUtcNow())
        {
            throw new ValidationException("Activation is invalid");
        }

        existingAccount.AccountStatus = AccountStatus.active;
        existingAccount.ActivatedUtc = _dateTimeProvider.GetUtcNow();
        existingAccount.UpdatedUtc = _dateTimeProvider.GetUtcNow();

        bool activated = await _accountRepository.ActivateAsync(existingAccount, token);

        return activated;
    }

    public async Task<bool> ResendActivationAsync(AccountActivation activationRequest, CancellationToken token = default)
    {
        Account? account = await _accountRepository.GetByUsernameAsync(activationRequest.Username, token);

        if (account is null)
        {
            throw new ValidationException("No account found");
        }

        if (!string.Equals(account.ActivationCode, activationRequest.ActivationCode))
        {
            throw new ValidationException("Activation code invalid");
        }

        await CreateActivationData(account, token);

        bool success = await _accountRepository.UpdateActivationAsync(account, token);

        if (!success)
        {
            return false;
        }

        try
        {
            await QueueActivationEmail(account, token);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
        }

        return success;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _accountRepository.ExistsByIdAsync(id, token);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default)
    {
        return await _accountRepository.ExistsByEmailAsync(email, token);
    }

    public async Task<bool> RequestPasswordReset(string email, CancellationToken token = default)
    {
        Account? account = await _accountRepository.GetByEmailAsync(email, token);

        if (account is null)
        {
            return false;
        }

        // already requested at one point
        if (account.PasswordResetRequestedUtc.HasValue)
        {
            TimeSpan duration = (_dateTimeProvider.GetUtcNow() - account.PasswordResetRequestedUtc.Value).Duration();
            int maxPasswordResetMinutes = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.PASSWORD_REQUEST_DURATION_MINS, 5, token);

            if (duration < TimeSpan.FromMinutes(maxPasswordResetMinutes))
            {
                return true; // do nothing as they're still in the time window 
            }
        }

        account.ResetCode = _passwordHasher.CreateOneTimeCode();

        await _accountRepository.RequestPasswordResetAsync(email, account.ResetCode, token);
        
        try
        {
            await QueuePasswordResetEmail(account, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        return true;
    }

    public async Task<bool> VerifyPasswordResetCode(string email, string code, CancellationToken token = default)
    {
        Account? account = await _accountRepository.GetByEmailAsync(email, token);

        if (account?.PasswordResetRequestedUtc == null)
        {
            throw new ValidationException("Reset was not requested");
        }

        int maxPasswordResetMins = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.PASSWORD_RESET_REQUEST_EXPIRATION_MINS, 5, token);

        if ((_dateTimeProvider.GetUtcNow() - account.PasswordResetRequestedUtc.Value).Duration() > TimeSpan.FromMinutes(maxPasswordResetMins))
        {
            throw new ValidationException("Code has expired");
        }

        if (!string.Equals(account.ResetCode, code))
        {
            throw new ValidationException("Code has expired");
        }

        return true;
    }

    public async Task<bool> ResetPassword(PasswordReset reset, CancellationToken token = default)
    {
        await _passwordResetValidator.ValidateAndThrowAsync(reset, token);

        reset.Password = _passwordHasher.Hash(reset.Password); // gotta have it
        
        bool result = await _accountRepository.ResetPasswordAsync(reset, token);

        return result;
    }

    private async Task QueueActivationEmail(Account account, CancellationToken token = default)
    {
        string? serviceUsername = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.SERVICE_ACCOUNT_USERNAME, string.Empty, token);
        string? emailFormat = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.ACTIVATION_EMAIL_FORMAT, string.Empty, token);
        int expirationMinutes = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.ACCOUNT_ACTIVATION_EXPIRATION_MINS, 5, token);

        Account? serviceAccount = await _accountRepository.GetByUsernameAsync(serviceUsername, token);

        string activationLink = string.Format(ApplicationAssumptions.ACTIVATION_LINK_FORMAT, account.Username, account.ActivationCode);
        string resendLink = string.Format(ApplicationAssumptions.RESEND_ACTIVATION_LINK_FORMAT, account.Username, account.ActivationCode);
        
        EmailData data = new()
                         {
                             Id = Guid.NewGuid(),
                             ShouldSend = true,
                             SendAttempts = 0,
                             SendAfterUtc = _dateTimeProvider.GetUtcNow(),
                             SenderAccountId = serviceAccount.Id,
                             ReceiverAccountId = account.Id,
                             SenderEmail = serviceAccount.Email,
                             RecipientEmail = account.Email,
                             Body = string.Format(emailFormat, activationLink, resendLink, expirationMinutes),
                             ResponseLog = $"{_dateTimeProvider.GetUtcNow()}: Email created;"
                         };

        _emailService.QueueEmailAsync(data, token); // fire and forget, no waiting.
    }
    
    private async Task QueuePasswordResetEmail(Account account, CancellationToken token = default)
    {
        string? serviceUsername = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.SERVICE_ACCOUNT_USERNAME, string.Empty, token);
        string? emailFormat = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.PASSWORD_RESET_EMAIL_FORMAT, string.Empty, token);
        int expirationMinutes = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.PASSWORD_RESET_REQUEST_EXPIRATION_MINS, 5, token);

        Account? serviceAccount = await _accountRepository.GetByUsernameAsync(serviceUsername, token);
        
        EmailData data = new()
                         {
                             Id = Guid.NewGuid(),
                             ShouldSend = true,
                             SendAttempts = 0,
                             SendAfterUtc = _dateTimeProvider.GetUtcNow(),
                             SenderAccountId = serviceAccount.Id,
                             ReceiverAccountId = account.Id,
                             SenderEmail = serviceAccount.Email,
                             RecipientEmail = account.Email,
                             Body = string.Format(emailFormat, account.ResetCode, expirationMinutes),
                             ResponseLog = $"{_dateTimeProvider.GetUtcNow()}: Email created;"
                         };

        _emailService.QueueEmailAsync(data, token); // fire and forget, no waiting.
    }

    private async Task CreateActivationData(Account account, CancellationToken token)
    {
        int expirationMinutes = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.ACCOUNT_ACTIVATION_EXPIRATION_MINS, 5, token);

        account.ActivationExpiration = _dateTimeProvider.GetUtcNow().AddMinutes(expirationMinutes);
        account.ActivationCode = _passwordHasher.CreateActivationToken();
    }
}