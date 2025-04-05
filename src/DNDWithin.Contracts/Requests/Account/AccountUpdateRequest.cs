using DNDWithin.Contracts.Models;

namespace DNDWithin.Contracts.Requests.Account;

public class AccountUpdateRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public  string Username { get; set; }
    public  string Email { get; init; }
    public  string Password { get; init; }
    public required AccountStatus AccountStatus { get; init; }
    public required AccountRole AccountRole { get; init; }
}