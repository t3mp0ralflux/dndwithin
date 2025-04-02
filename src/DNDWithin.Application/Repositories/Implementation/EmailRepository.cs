using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models.System;
using DNDWithin.Application.Services.Implementation;

namespace DNDWithin.Application.Repositories.Implementation;

public class EmailRepository : IEmailRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EmailRepository(IDbConnectionFactory dbConnectionFactory, IDateTimeProvider dateTimeProvider)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> QueueEmail(EmailData emailData, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        emailData.ResponseLog += $"{_dateTimeProvider.GetUtcNow()}: Email Queued;";

        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into email (id, account_id_sender, account_id_receiver, should_send, send_after_utc, sender_email, recipient_email, body, response_log)
                                                                         values (@Id, @SenderAccountId, @ReceiverAccountId, @ShouldSend, @SendAfterUtc, @SenderEmail, @RecipientEmail, @Body, @ResponseLog)
                                                                         """, emailData, cancellationToken: token));

        return result > 0;
    }

    public Task<List<EmailData>> GetForProcessing(int batchSize, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Update(EmailData emailData, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}