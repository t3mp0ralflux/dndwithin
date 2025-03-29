using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.GlobalSettings;

namespace DNDWithin.Application.Repositories;

public interface IGlobalSettingsRepository
{
    Task<bool> CreateSetting(string name, string value, CancellationToken token = default);
    Task<GlobalSetting?> GetSetting(string name, CancellationToken token = default);
    Task<IEnumerable<GlobalSetting>> GetAllAsync(GetAllGlobalSettingsOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(string name, CancellationToken token = default);
}