namespace DNDWithin.Application.Models.Auth;

public class PasswordReset
{
    public required string Email { get; init; }
    public required string ResetCode { get; set; }
    public required string Password { get; set; }
}