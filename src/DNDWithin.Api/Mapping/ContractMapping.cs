using DNDWithin.Application.Models;
using DNDWithin.Contracts.Requests.Account;
using DNDWithin.Contracts.Responses.Account;
using ctr = DNDWithin.Contracts.Models;

namespace DNDWithin.Api.Mapping;

public static class ContractMapping
{
    #region Accounts
    public static Account ToAccount(this AccountCreateRequest request)
    {
        return new Account()
               {
                   Id = Guid.NewGuid(),
                   FirstName = request.FirstName,
                   LastName = request.LastName,
                   UserName = request.UserName,
                   Password = request.Password,
                   Email = request.Email
               };
    }

    public static AccountResponse ToResponse(this Account account)
    {
        return new AccountResponse
               {
                    Id = account.Id,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Email = account.Email,
                    UserName = account.UserName,
                    AccountRole = (ctr.AccountRole)account.AccountRole,
                    AccountStatus = (ctr.AccountStatus)account.AccountStatus,
                    LastLogin = account.LastLoginUtc
               };
    }
    
    public static AccountsResponse ToResponse(this IEnumerable<Account> accounts, int page, int pageSize, int totalCount)
    {
        return new AccountsResponse()
               {
                   Items = accounts.Select(ToResponse),
                   Page = page,
                   PageSize = pageSize,
                   Total = totalCount
               };
    }

    public static GetAllAccountsOptions ToOptions(this GetAllAccountsRequest request)
    {
        string? sortField = request.SortBy?.Trim('+', '-');
        if (sortField is not null && sortField == "lastlogin")
        {
            sortField = "last_login_utc";
        }

        return new GetAllAccountsOptions()
               {
                   UserName = request.UserName,
                   AccountStatus = (AccountStatus?)request.AccountStatus,
                   AccountRole = (AccountRole?)request.AccountRole,
                   SortField = sortField,
                   SortOrder = request.SortBy is null ? SortOrder.unordered : request.SortBy.StartsWith('-') ? SortOrder.descending : SortOrder.ascending,
                   Page = request.Page,
                   PageSize = request.PageSize
               };
    }
    #endregion
}
