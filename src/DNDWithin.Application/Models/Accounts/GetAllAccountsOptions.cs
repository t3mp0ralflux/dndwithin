namespace DNDWithin.Application.Models.Accounts;

public class GetAllAccountsOptions : SharedGetAllOptions
{
    public string? UserName { get; init; }
    public AccountStatus? AccountStatus { get; init; }
    public AccountRole? AccountRole { get; init; }

}