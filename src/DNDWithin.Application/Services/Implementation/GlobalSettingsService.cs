using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Application.Repositories;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace DNDWithin.Application.Services.Implementation;

public class GlobalSettingsService : IGlobalSettingsService
{
    private readonly IGlobalSettingsRepository _globalSettingsRepository;
    private readonly IValidator<GlobalSetting> _globalSettingValidator;
    private readonly IValidator<GetAllGlobalSettingsOptions> _optionsValidator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMemoryCache _cache;

    public GlobalSettingsService(IGlobalSettingsRepository globalSettingsRepository, IValidator<GlobalSetting> globalSettingValidator, IValidator<GetAllGlobalSettingsOptions> optionsValidator, IDateTimeProvider dateTimeProvider, IMemoryCache cache)
    {
        _globalSettingsRepository = globalSettingsRepository;
        _globalSettingValidator = globalSettingValidator;
        _optionsValidator = optionsValidator;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
    }

    public async Task<bool> CreateSettingAsync(GlobalSetting setting, CancellationToken token = default)
    {
        await _globalSettingValidator.ValidateAndThrowAsync(setting, token);

        return await _globalSettingsRepository.CreateSetting(setting, token);
    }

    public async Task<IEnumerable<GlobalSetting>> GetAllAsync(GetAllGlobalSettingsOptions options, CancellationToken token = default)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, token);

        return await _globalSettingsRepository.GetAllAsync(options, token);
    }

    public async Task<int> GetCountAsync(string name, CancellationToken token = default)
    {
        return await _globalSettingsRepository.GetCountAsync(name, token);
    }

    public async Task<GlobalSetting?> GetSettingAsync(string name, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        return setting;
    }

    public async Task<T?> GetSettingAsync<T>(string name, T? defaultValue, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        if (setting is null)
        {
            return defaultValue;
        }

        try
        {
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }
        catch (Exception ex)
        {
            // TODO:MAYBE: log this?
            return defaultValue;
        }
    }

    public async Task<T?> GetSettingCachedAsync<T>(string name, T? defaultValue, CancellationToken token = default)
    {
        if (_cache.TryGetValue(name, out T? data))
        {
            return data;
        }

        data = await GetSettingAsync(name, defaultValue, token);

        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(name, data, options);

        return data;
    }
}