namespace DNDWithin.Contracts.Requests.Account;

public class AccountActivationRequest
{
    public required string Username { get; set; }
    public required string ActivationCode { get; set; }
}