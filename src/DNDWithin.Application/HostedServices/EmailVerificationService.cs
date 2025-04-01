using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DNDWithin.Application.HostedServices;

public class EmailVerificationService : IHostedService
{
    private readonly ILogger<EmailVerificationService> _logger;
    private PeriodicTimer _timer;

    public EmailVerificationService(ILogger<EmailVerificationService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailVerification Service started");

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        State state = new();

        bool keepRunning = true;

        while (keepRunning)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                keepRunning = false;
            }

            await SendEmailsAsync(state);
            await _timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        _logger.LogInformation("EmailVerification Service ended");
        return Task.CompletedTask;
    }

    private async Task SendEmailsAsync(State state)
    {
        if (state.Count >= 5)
        {
            state.IsRunning = false;
        }

        _logger.LogInformation("Is Running: {StateIsRunning}", state.IsRunning);
        if (state.IsRunning)
        {
            _logger.LogInformation("Won't start another run, already running");
            _logger.LogInformation("Email sent");
            state.Count++;
            return;
        }

        state.IsRunning = true;
        _logger.LogInformation("Starting email processing");
    }

    private class State
    {
        public bool IsRunning { get; set; }
        public int Count { get; set; }
        public CancellationToken token { get; set; }
    }
}