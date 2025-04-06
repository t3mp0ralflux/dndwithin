using DNDWithin.Application.Models.System;

namespace DNDWithin.Application.Repositories;

public interface IEmailRepository
{
    Task<bool> QueueEmailAsync(EmailData emailData, CancellationToken token = default);
    Task<List<EmailData>> GetForProcessingAsync(int batchSize, CancellationToken token = default);
    Task<bool> UpdateAsync(EmailData emailData, CancellationToken token = default);
}