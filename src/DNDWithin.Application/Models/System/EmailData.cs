namespace DNDWithin.Application.Models.System;

public class EmailData
{
    public required Guid Id { get; set; }
    public required Guid SenderAccountId { get; set; }
    public required Guid ReceiverAccountId { get; set; }
    public required bool ShouldSend { get; set; } = true;
    public required DateTime SendAfterUtc { get; set; }
    public required string SenderEmail { get; set; }
    public required string RecipientEmail { get; set; }
    public required string Body { get; set; }
    public required string ResponseLog { get; set; }
}