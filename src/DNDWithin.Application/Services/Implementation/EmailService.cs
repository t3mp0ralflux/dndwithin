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

    public async Task QueueEmail(EmailData emailData, CancellationToken token = default)
    {
        emailData.ResponseLog += $"{_dateTimeProvider.GetUtcNow()}: Email Queued;";

        await _emailRepository.QueueEmail(emailData, token);
    }

    public async Task<List<EmailData>> GetForProcessing(int batchSize, CancellationToken token = default)
    {
        return await _emailRepository.GetForProcessing(batchSize, token);
    }

    public async Task<bool> Update(EmailData emailData, CancellationToken token = default)
    {
        return await _emailRepository.Update(emailData, token);
    }
}