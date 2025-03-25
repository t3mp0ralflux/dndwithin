using System.Globalization;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.GlobalSettings;

namespace DNDWithin.Application.Services;

public interface IGlobalSettingsService
{
    Task<bool> CreateSettingAsync(string name, string value, CancellationToken token = default);
    Task<IEnumerable<GlobalSetting>> GetAllAsync(GetAllGlobalSettingsOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(string name, CancellationToken token = default);
    Task<bool> GetSettingAsync(string name, bool defaultValue, CancellationToken token = default);
    Task<Guid> GetSettingAsync(string name, Guid defaultValue, CancellationToken token = default);
    Task<int> GetSettingAsync(string name, int defaultValue, CancellationToken token = default);
    Task<double> GetSettingAsync(string name, double defaultValue, CancellationToken token = default);
    Task<DateTime> GetSettingAsync(string name, DateTime defaultValue, CancellationToken token = default);
}
