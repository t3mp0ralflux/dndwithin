using DNDWithin.Contracts.Models;

namespace DNDWithin.Contracts.Requests.Account;

public class AccountUpdateRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required AccountStatus AccountStatus { get; init; }
    public required AccountRole AccountRole { get; init; }
}