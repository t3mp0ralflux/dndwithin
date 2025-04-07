using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Contracts.Requests.Account;
using DNDWithin.Contracts.Requests.Characters;
using DNDWithin.Contracts.Requests.GlobalSetting;
using DNDWithin.Contracts.Responses.Account;
using DNDWithin.Contracts.Responses.Characters;
using DNDWithin.Contracts.Responses.GlobalSetting;
using Microsoft.AspNetCore.WebUtilities;
using ctr = DNDWithin.Contracts.Models;

namespace DNDWithin.Api.Mapping;

public static class ContractMapping
{
    #region Accounts

    public static Account ToAccount(this AccountCreateRequest request)
    {
        return new Account
               {
                   Id = Guid.NewGuid(),
                   FirstName = request.FirstName,
                   LastName = request.LastName,
                   Username = request.UserName,
                   Password = request.Password,
                   Email = request.Email.ToLowerInvariant()
               };
    }

    public static Account ToAccount(this AccountUpdateRequest request, Guid id)
    {
        return new Account
               {
                   Id = id,
                   FirstName = request.FirstName,
                   LastName = request.LastName,
                   Username = string.Empty, // not used, but required
                   Email = string.Empty, // not used, but required
                   Password = string.Empty, // not used, but required
                   AccountStatus = (AccountStatus)request.AccountStatus,
                   AccountRole = (AccountRole)request.AccountRole
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
        return new AccountsResponse
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

        return new GetAllAccountsOptions
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

    public static AccountActivationResponse ToResponse(this AccountActivation activation)
    {
        return new AccountActivationResponse
               {
                   Username = activation.Username
               };
    }

    #endregion

    #region Characters

    public static Character ToCharacter(this CharacterUpdateRequest request, Account account)
    {
        return new Character()
               {
                   Id = request.Id,
                   AccountId = account.Id,
                   Name = request.Name,
                   Username = account.Username,
                   Characteristics = new Characteristics()
                                     {
                                         Age = request.Age,
                                         Eyes = request.Eyes,
                                         Gender = request.Gender,
                                         Hair = request.Hair,
                                         Height = request.Height,
                                         Skin = request.Height,
                                         Weight = request.Weight
                                     }
               };
    }

    public static CharactersResponse ToGetAllResponse(this IEnumerable<Character> characters, int page, int pageSize, int total)
    {
        return new CharactersResponse()
               {
                   Items = characters.Select(x => new GetAllCharacterResponse()
                                                  {
                                                      AccountId = x.AccountId,
                                                      Id = x.Id,
                                                      Name = x.Name,
                                                      Username = x.Username
                                                  }),
                   Page = page,
                   PageSize = pageSize,
                   Total = total
               };
    }

    public static CharacterResponse ToResponse(this Character character)
    {
        return new CharacterResponse()
               {
                   Id = character.Id,
                   AccountId = character.AccountId,
                   Age = character.Characteristics.Age,
                   Eyes = character.Characteristics.Eyes,
                   Gender = character.Characteristics.Gender,
                   Hair = character.Characteristics.Hair,
                   Height = character.Characteristics.Height,
                   Name = character.Name,
                   Skin = character.Characteristics.Skin,
                   Username = character.Username
               };
    }

    public static GetAllCharactersOptions ToOptions(this GetAllCharactersRequest request, Guid accountId)
    {
        return new GetAllCharactersOptions()
               {
                   AccountId = accountId,
                   Name = request.Name,
                   Class = request.Class,
                   Level = request.Level,
                   Species = request.Species,
                   Page = request.Page,
                   PageSize = request.PageSize,
                   SortField = request.SortBy,
                   SortOrder = request.SortBy is null ? SortOrder.unordered : request.SortBy.StartsWith('-') ? SortOrder.descending : SortOrder.ascending
               };
    }

    #endregion

    #region GlobalSettings

    public static GlobalSetting ToGlobalSetting(this GlobalSettingCreateRequest request)
    {
        return new GlobalSetting
               {
                   Id = Guid.NewGuid(),
                   Name = request.Name,
                   Value = request.Value
               };
    }

    public static GetAllGlobalSettingsOptions ToOptions(this GetAllGlobalSettingsRequest request)
    {
        string? sortField = request.SortBy?.Trim('+', '-');

        return new GetAllGlobalSettingsOptions
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
        return new GlobalSettingResponse
               {
                   Id = globalSetting.Id,
                   Name = globalSetting.Name,
                   Value = globalSetting.Value
               };
    }

    public static GlobalSettingsResponse ToResponse(this IEnumerable<GlobalSetting> settings, int page, int pageSize, int totalCount)
    {
        return new GlobalSettingsResponse
               {
                   Items = settings.Select(ToResponse),
                   Page = page,
                   PageSize = pageSize,
                   Total = totalCount
               };
    }

    #endregion
}