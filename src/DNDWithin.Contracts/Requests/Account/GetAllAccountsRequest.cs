using DNDWithin.Contracts.Models;

namespace DNDWithin.Contracts.Requests.Account;

public class GetAllAccountsRequest : PagedRequest
{
    public string? UserName { get; init; }
    public AccountStatus? AccountStatus { get; init; }
    public AccountRole? AccountRole { get; init; }
    public string? SortBy { get; init; }
}