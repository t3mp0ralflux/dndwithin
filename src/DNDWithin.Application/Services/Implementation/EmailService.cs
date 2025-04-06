using DNDWithin.Application.Models.System;
using DNDWithin.Application.Repositories;

namespace DNDWithin.Application.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailRepository _emailRepository;

    public EmailService(IEmailRepository emailRepository, IDateTimeProvider dateTimeProvider)
    {
        _emailRepository = emailRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task QueueEmailAsync(EmailData emailData, CancellationToken token = default)
    {
        emailData.ResponseLog += $"{_dateTimeProvider.GetUtcNow()}: Email Queued;";

        await _emailRepository.QueueEmailAsync(emailData, token);
    }

    public async Task<List<EmailData>> GetForProcessingAsync(int batchSize, CancellationToken token = default)
    {
        return await _emailRepository.GetForProcessingAsync(batchSize, token);
    }

    public async Task<bool> UpdateAsync(EmailData emailData, CancellationToken token = default)
    {
        return await _emailRepository.UpdateAsync(emailData, token);
    }
}