using DNDWithin.Application.Models.System;
using DNDWithin.Application.Services;
using DNDWithin.Application.Services.Implementation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DNDWithin.Application.HostedServices;

public class EmailVerificationService : IHostedService
{
    private readonly ILogger<EmailVerificationService> _logger;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGlobalSettingsService _globalSettingsService;
    
    private PeriodicTimer _timer;

    public EmailVerificationService(ILogger<EmailVerificationService> logger, IEmailService emailService, IDateTimeProvider dateTimeProvider, IGlobalSettingsService globalSettingsService)
    {
        _logger = logger;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
        _globalSettingsService = globalSettingsService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailVerification Service started");

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        State state = new();

        while (!cancellationToken.IsCancellationRequested)
        {
            await ProcessEmailsAsync(state, cancellationToken);
            await _timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        _logger.LogInformation("EmailVerification Service ended");
        return Task.CompletedTask;
    }

    private async Task ProcessEmailsAsync(State state, CancellationToken token)
    {
        if (state.IsRunning)
        {
            return;
        }

        var maxEmailsToSend = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.EMAIL_SEND_BATCH_LIMIT, 100, token);

        List<EmailData> emailsToProcess = await _emailService.GetForProcessing(maxEmailsToSend, token);

        if (emailsToProcess.Count == 0)
        {
            state.IsRunning = false;
            return;
        }

        var maxAttempts = await _globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.EMAIL_SEND_ATTEMPTS_MAX, 5, token);

        foreach (EmailData emailData in emailsToProcess)
        {
            emailData.SendAttempts++;

            if (emailData.SendAttempts > maxAttempts)
            {
                emailData.ShouldSend = false;
                emailData.ResponseLog += $"{_dateTimeProvider}: Max email attempts reached";

                _emailService.Update(emailData, token);
                continue;
            }

            emailData.ShouldSend = false; // hit early to avoid spamming on DB write errors.
            emailData.ResponseLog += $"{_dateTimeProvider}: Email Sent;";

            _emailService.Update(emailData, token);

            var success = await SendEmailAsync(emailData, token);

            if (success)
            {
                continue;
            }

            emailData.ShouldSend = true;
            emailData.ResponseLog += $"{_dateTimeProvider.GetUtcNow()}: Email failed to send. Attempt {emailData.SendAttempts} out of {maxAttempts};";
            _emailService.Update(emailData, token);
        }
    }

    private async Task<bool> SendEmailAsync(EmailData emailData, CancellationToken token)
    {
        return true;
    }

    private class State
    {
        public bool IsRunning { get; set; }
        public int Count { get; set; }
        public CancellationToken token { get; set; }
    }
}