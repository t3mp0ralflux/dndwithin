using DNDWithin.Application.Models.System;

namespace DNDWithin.Application.Services;

public interface IEmailService
{
    Task SendEmail(EmailData emailData, CancellationToken token = default);
}
