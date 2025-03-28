using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Contracts.Requests.Account;
using DNDWithin.Contracts.Requests.GlobalSetting;
using DNDWithin.Contracts.Responses.Account;
using DNDWithin.Contracts.Responses.GlobalSetting;
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
                   Username = request.UserName,
                   Password = request.Password,
                   Email = request.Email
               };
    }

    public static Account ToAccount(this AccountUpdateRequest request, Guid id)
    {
        return new Account()
               {
                   Id = id,
                   FirstName = request.FirstName,
                   LastName = request.LastName,
                   AccountStatus = (AccountStatus)request.AccountStatus,
                   AccountRole = (AccountRole)request.AccountRole,
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
                    UserName = account.Username,
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

    #region GlobalSettings

    public static GetAllGlobalSettingsOptions ToOptions(this GetAllGlobalSettingsRequest request)
    {
        string? sortField = request.SortBy?.Trim('+', '-');

        return new GetAllGlobalSettingsOptions()
               {
                   Name = request.Name,
                   SortField = sortField,
                   SortOrder = request.SortBy is null ? SortOrder.unordered : request.SortBy.StartsWith('-') ? SortOrder.descending : SortOrder.ascending,
                   Page = request.Page,
                   PageSize = request.PageSize
               };
    }

    public static GlobalSettingResponse ToResponse(this GlobalSetting globalSetting)
    {
        return new GlobalSettingResponse()
               {
                   Id = globalSetting.Id,
                   Name = globalSetting.Name,
                   Value = globalSetting.Value
               };
    }

    public static GlobalSettingsResponse ToResponse(this IEnumerable<GlobalSetting> settings, int page, int pageSize, int totalCount)
    {
        return new GlobalSettingsResponse()
               {
                   Items = settings.Select(ToResponse),
                   Page = page,
                   PageSize = pageSize,
                   Total = totalCount
               };
    }
    #endregion
}
