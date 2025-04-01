namespace DNDWithin.Application.Models.Accounts;

public class AccountActivation
{
    public required string Username { get; set; }
    public required string ActivationCode { get; set; }
    public DateTime? Expiration { get; set; }
}