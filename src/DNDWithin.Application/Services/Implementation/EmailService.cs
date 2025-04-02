using DNDWithin.Application.Models.System;

namespace DNDWithin.Application.Services.Implementation;

public class EmailService : IEmailService
{
    public async Task QueueEmail(EmailData emailData, CancellationToken token = default)
    {
        // TODO: FOR NOW: 
        Console.WriteLine("I'm queueing an email!");
        Console.WriteLine($"To: {emailData.RecipientEmail}, From: {emailData.SenderEmail}, Body: {emailData.Body}");
    }
}