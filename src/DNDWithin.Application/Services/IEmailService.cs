using DNDWithin.Application.Models.System;

namespace DNDWithin.Application.Services;

public interface IEmailService
{
    Task QueueEmail(EmailData emailData, CancellationToken token = default);
}
