namespace DNDWithin.Application.Models.System;

public class EmailData
{
    public required Guid Id { get; init; }
    public required Guid SenderAccountId { get; init; }
    public required Guid ReceiverAccountId { get; init; }
    public required bool ShouldSend { get; set; } = true;
    public required int SendAttempts { get; set; }
    public DateTime? SentUtc { get; set; }
    public required DateTime SendAfterUtc { get; set; }
    public required string SenderEmail { get; set; }
    public required string RecipientEmail { get; set; }
    public required string Body { get; set; }
    public required string ResponseLog { get; set; }
}