namespace DNDWithin.Contracts.Requests.Auth;

public class PasswordResetVerification
{
    public required string Email { get; init; }
    public required string Code { get; init; }
}