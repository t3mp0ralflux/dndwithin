using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.System;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Services.Implementation;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IValidator<Account> _accountValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;
    private readonly IGlobalSettingsService _globalSettingsService;
    private readonly IValidator<GetAllAccountsOptions> _optionsValidator;
    private readonly IPasswordHasher _passwordHasher;

    public AccountService(IAccountRepository accountRepository, IValidator<Account> accountValidator, IDateTimeProvider dateTimeProvider, IValidator<GetAllAccountsOptions> optionsValidator, IPasswordHasher passwordHasher, IGlobalSettingsService globalSettingsService, IEmailService emailService)
    {
        _accountRepository = accountRepository;
        _accountValidator = accountValidator;
        _dateTimeProvider = dateTimeProvider;
        _optionsValidator = optionsValidator;
        _passwordHasher = passwordHasher;
        _globalSettingsService = globalSettingsService;
        _emailService = emailService;
    }

    public async Task<bool> CreateAsync(Account account, CancellationToken token = default)
    {
        await _accountValidator.ValidateAndThrowAsync(account, token);
        account.CreatedUtc = _dateTimeProvider.GetUtcNow();
        account.UpdatedUtc = _dateTimeProvider.GetUtcNow();
        account.Password = _passwordHasher.Hash(account.Password!);

        int expirationHours = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.ACCOUNT_ACTIVATION_EXPIRATION_HOURS, 8, token);

        AccountActivation activation = new()
                                       {
                                           Username = account.Username,
                                           Expiration = _dateTimeProvider.GetUtcNow().AddHours(expirationHours),
                                           ActivationCode = _passwordHasher.CreateActivationToken()
                                       };

        bool success = await _accountRepository.CreateAsync(account, activation, token);

        if (success)
        {
            string? linkFormat = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.ACTIVATION_LINK_FORMAT, string.Empty, token);
            if (string.IsNullOrWhiteSpace(linkFormat))
            {
                throw new Exception("Activation Link not found"); // can't do anything with nothing. TODO:MUST: make this less visible to the end user and log this exception to Grafana or Jenkins or whatever.
            }

            string? serviceUsername = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.SERVICE_ACCOUNT_USERNAME, string.Empty, token);

            if (string.IsNullOrWhiteSpace(serviceUsername))
            {
                throw new Exception("Service Account username not found"); // TODO:MUST: make this less visible to the end user and log this exception to Grafana or Jenkins or whatever.
            }

            Account? serviceAccount = await _accountRepository.GetByUsernameAsync(serviceUsername, token);
            if (serviceAccount is null)
            {
                throw new Exception("Service Account not found"); // TODO:MUST: make this less visible to the end user and log this exception to Grafana or Jenkins or whatever.
            }

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
                                 Body = string.Format(linkFormat, account.Username, activation.ActivationCode), // TODO:MUST: this will be pre-formatted HTML for emails with standard warnings in it.
                                 ResponseLog = $"{_dateTimeProvider.GetUtcNow()}: Email created;"
                             };

            _emailService.QueueEmail(data, token); // fire and forget, no waiting.
        }

        return success;
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
        return await _accountRepository.GetByUsernameAsync(userName.ToLowerInvariant(), token);
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

    public async Task<(bool isActive, string reason)> ActivateAsync(AccountActivation activation, CancellationToken token = default)
    {
        Account? existingAccount = await _accountRepository.GetByUsernameAsync(activation.Username.ToLowerInvariant(), token);

        if (existingAccount is null)
        {
            return (false, "No account found");
        }

        if (existingAccount.Activation.Code != activation.ActivationCode || existingAccount.Activation.Expiration < _dateTimeProvider.GetUtcNow())
        {
            return (false, "Activation Code has expired");
        }

        existingAccount.AccountStatus = AccountStatus.active;
        existingAccount.ActivatedUtc = _dateTimeProvider.GetUtcNow();
        existingAccount.UpdatedUtc = _dateTimeProvider.GetUtcNow();

        bool activated = await _accountRepository.ActivateAsync(existingAccount, token);

        return !activated
            ? (false, "Activation failed, please try again")
            : (activated, "Activation successful");
    }
}