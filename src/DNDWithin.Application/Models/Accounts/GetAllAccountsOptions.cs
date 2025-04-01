namespace DNDWithin.Application.Models.Accounts;

public class GetAllAccountsOptions
{
    public string? UserName { get; init; }
    public AccountStatus? AccountStatus { get; init; }
    public AccountRole? AccountRole { get; init; }
    public string? SortField { get; init; }
    public SortOrder? SortOrder { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}