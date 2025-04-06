using DNDWithin.Application.Models.System;

namespace DNDWithin.Application.Services;

public interface IEmailService
{
    Task QueueEmail(EmailData emailData, CancellationToken token = default);
    Task<List<EmailData>> GetForProcessing(int batchSize, CancellationToken token = default);
    Task<bool> Update(EmailData emailData, CancellationToken token = default);
}