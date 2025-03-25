namespace DNDWithin.Application.Models.Accounts;

public class GetAllAccountsOptions
{
    public string? UserName { get; set; }
    public AccountStatus? AccountStatus { get; set; }
    public AccountRole? AccountRole { get; set; }
    public string? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}