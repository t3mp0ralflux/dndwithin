namespace DNDWithin.Contracts.Requests.Auth;

public class PasswordResetRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string ResetCode { get; init; }
    
}