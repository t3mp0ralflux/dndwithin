using DNDWithin.Application.Models.System;

namespace DNDWithin.Application.Services.Implementation;

public class EmailService : IEmailService
{
    public async Task SendEmail(EmailData emailData, CancellationToken token = default)
    {
        // TODO: FOR NOW: 
        Console.WriteLine("I'm sending an email!");
        Console.WriteLine($"To: {emailData.RecipientEmail}, From: {emailData.SenderEmail}, Body: {emailData.Body}");
    }
}