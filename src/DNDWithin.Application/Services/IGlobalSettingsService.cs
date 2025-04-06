using DNDWithin.Application.Models.GlobalSettings;

namespace DNDWithin.Application.Services;

public interface IGlobalSettingsService
{
    Task<bool> CreateSettingAsync(GlobalSetting setting, CancellationToken token = default);
    Task<IEnumerable<GlobalSetting>> GetAllAsync(GetAllGlobalSettingsOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(string name, CancellationToken token = default);
    Task<GlobalSetting?> GetSettingAsync(string name, bool defaultValue, CancellationToken token = default);
    Task<T?> GetSettingAsync<T>(string name, T? defaultValue, CancellationToken token = default);
}