﻿using DNDWithin.Application.Models.Accounts;
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

    private async Task QueueActivationEmail(Account account, CancellationToken token = default)
    {
        string? linkFormat = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.ACTIVATION_LINK_FORMAT, string.Empty, token);
        string? emailFormat = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.ACTIVATION_EMAIL_FORMAT, string.Empty, token);
        string? serviceUsername = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.SERVICE_ACCOUNT_USERNAME, string.Empty, token);
        int expirationMinutes = await _globalSettingsService.GetSettingCachedAsync(WellKnownGlobalSettings.ACCOUNT_ACTIVATION_EXPIRATION_MINS, 5, token);

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
                             Body = string.Format(emailFormat, string.Format(linkFormat, account.Username, account.ActivationCode), expirationMinutes), // TODO:MUST: this will be pre-formatted HTML for emails with standard warnings in it.
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