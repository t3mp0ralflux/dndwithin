using DNDWithin.Application.Models.Accounts;
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

    public AccountService(IAccountRepository accountRepository, IValidator<Account> accountValidator, IDateTimeProvider dateTimeProvider, IValidator<GetAllAccountsOptions> optionsValidator, IPasswordHasher passwordHasher, IGlobalSettingsService globalSettingsService, IEmailService emailService, ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _accountValidator = accountValidator;
        _dateTimeProvider = dateTimeProvider;
        _optionsValidator = optionsValidator;
        _passwordHasher = passwordHasher;
        _globalSettingsService = globalSettingsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token = default)
    {
        await _accountValidator.ValidateAndThrowAsync(account, token);
        account.CreatedUtc = _dateTimeProvider.GetUtcNow();
        account.UpdatedUtc = _dateTimeProvider.GetUtcNow();
        account.Password = _passwordHasher.Hash(account.Password);

        (int expirationMinutes, AccountActivation activation) = await CreateActivationData(account, token);

        bool success = await _accountRepository.CreateAsync(account, activation, token);

        if (!success)
        {
            return false;
        }

        try
        {
            await QueueActivationEmail(account, activation, expirationMinutes, token);
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

    public async Task<int> GetCountAsync(string? userName, CancellationToken token = default)
    {
        return await _accountRepository.GetCountAsync(userName, token);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken token = default)
    {
        string emailLowered = email.ToLowerInvariant();

        return await _accountRepository.GetByEmailAsync(emailLowered, token);
    }

    public async Task<Account?> GetByUsernameAsync(string userName, CancellationToken token = default)
    {
        string usernameLowered = userName.ToLowerInvariant();

        return await _accountRepository.GetByUsernameAsync(usernameLowered, token);
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
        Account? existingAccount = await _accountRepository.GetByUsernameAsync(activation.Username.ToLowerInvariant(), token);

        if (existingAccount is null)
        {
            throw new ValidationException("No account found");
        }

        if (existingAccount.Activation.Code != activation.ActivationCode || existingAccount.Activation.Expiration < _dateTimeProvider.GetUtcNow())
        {
            throw new ValidationException("Activation code invalid");
        }

        existingAccount.AccountStatus = AccountStatus.active;
        existingAccount.ActivatedUtc = _dateTimeProvider.GetUtcNow();
        existingAccount.UpdatedUtc = _dateTimeProvider.GetUtcNow();

        bool activated = await _accountRepository.ActivateAsync(existingAccount, token);

        return activated;
    }

    public async Task<bool> ResendActivation(AccountActivation activationRequest, CancellationToken token = default)
    {
        Account? account = await _accountRepository.GetByUsernameAsync(activationRequest.Username, token);

        if (account is null)
        {
            throw new ValidationException("No account found");
        }

        if (!string.Equals(account.Activation.Code, activationRequest.ActivationCode))
        {
            throw new ValidationException("Activation code invalid");
        }

        (int expirationMinutes, AccountActivation activation) = await CreateActivationData(account, token);

        bool success = await _accountRepository.UpdateActivationAsync(account.Id, activation, token);

        if (!success)
        {
            return false;
        }

        try
        {
            await QueueActivationEmail(account, activation, expirationMinutes, token);
        }
        catch (Exception e)
        {   
            _logger.LogError(e.Message, e);
        }

        return success;
    }

    private async Task QueueActivationEmail(Account account, AccountActivation activation, int expirationMinutes, CancellationToken token)
    {
        string? linkFormat = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.ACTIVATION_LINK_FORMAT, string.Empty, token);
        string? emailFormat = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.ACTIVATION_EMAIL_FORMAT, string.Empty, token);
        string? serviceUsername = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.SERVICE_ACCOUNT_USERNAME, string.Empty, token);

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
                             Body = string.Format(emailFormat, string.Format(linkFormat, account.Username, activation.ActivationCode), expirationMinutes), // TODO:MUST: this will be pre-formatted HTML for emails with standard warnings in it.
                             ResponseLog = $"{_dateTimeProvider.GetUtcNow()}: Email created;"
                         };

        _emailService.QueueEmail(data, token); // fire and forget, no waiting.
    }

    private async Task<(int expirationMinutes, AccountActivation activation)> CreateActivationData(Account account, CancellationToken token)
    {
        int expirationMinutes = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.ACCOUNT_ACTIVATION_EXPIRATION_MINS, 5, token);

        AccountActivation activation = new()
                                       {
                                           Username = account.Username,
                                           Expiration = _dateTimeProvider.GetUtcNow().AddMinutes(expirationMinutes),
                                           ActivationCode = _passwordHasher.CreateActivationToken()
                                       };
        return (expirationMinutes, activation);
    }
}