using DNDWithin.Contracts.Models;

namespace DNDWithin.Contracts.Responses.Account;

public class AccountResponse
{
    public required Guid Id { get; set; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public AccountRole AccountRole { get; set; }
    public DateTime LastLogin { get; set; }
}