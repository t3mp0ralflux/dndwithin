using System.Data;
using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models.System;
using DNDWithin.Application.Services.Implementation;

namespace DNDWithin.Application.Repositories.Implementation;

public class EmailRepository : IEmailRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public EmailRepository(IDbConnectionFactory dbConnectionFactory, IDateTimeProvider dateTimeProvider)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> QueueEmail(EmailData emailData, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into email (id, account_id_sender, account_id_receiver, should_send, sent_utc, send_after_utc, sender_email, recipient_email, body, response_log, send_attempts)
                                                                         values (@Id, @SenderAccountId, @ReceiverAccountId, @ShouldSend, @SentUtc, @SendAfterUtc, @SenderEmail, @RecipientEmail, @Body, @ResponseLog, @SendAttempts)
                                                                         """, emailData, cancellationToken: token));

        transaction.Commit();

        return result > 0;
    }

    public async Task<List<EmailData>> GetForProcessing(int batchSize, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        IEnumerable<EmailData> result = await connection.QueryAsync<EmailData>(new CommandDefinition("""
                                                                                                     select id, account_id_sender as SenderAccountId, account_id_receiver as ReceiverAccountId, should_send as ShouldSend, sent_utc as SentUtc, send_after_utc as SendAfterUtc, sender_email as SenderEmail, recipient_email as RecipientEmail, body, response_log as ResponseLog, send_attempts as SendAttempts  
                                                                                                     from email
                                                                                                     where send_after_utc <= @Now
                                                                                                     and should_send = true
                                                                                                     limit @batchSize
                                                                                                     """, new { Now = _dateTimeProvider.GetUtcNow(), batchSize }, cancellationToken: token));

        return result.ToList();
    }

    public async Task<bool> Update(EmailData emailData, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update email
                                                                         set send_attempts = @SendAttempts, should_send = @ShouldSend, response_log = @ResponseLog, sent_utc = @SentUtc
                                                                         where id = @Id
                                                                         """, emailData, cancellationToken: token));

        transaction.Commit();

        return result > 0;
    }
}