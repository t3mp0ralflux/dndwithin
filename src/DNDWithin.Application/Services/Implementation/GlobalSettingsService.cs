using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Services.Implementation;

public class GlobalSettingsService : IGlobalSettingsService
{
    private readonly IGlobalSettingsRepository _globalSettingsRepository;
    private readonly IValidator<GlobalSetting> _globalSettingValidator;
    private readonly IValidator<GetAllGlobalSettingsOptions> _optionsValidator;

    public GlobalSettingsService(IGlobalSettingsRepository globalSettingsRepository, IValidator<GlobalSetting> globalSettingValidator, IValidator<GetAllGlobalSettingsOptions> optionsValidator)
    {
        _globalSettingsRepository = globalSettingsRepository;
        _globalSettingValidator = globalSettingValidator;
        _optionsValidator = optionsValidator;
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

    public async Task<GlobalSetting?> GetSettingAsync(string name, bool defaultValue, CancellationToken token = default)
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
            Type? type = Nullable.GetUnderlyingType(typeof(T));
            if (type is null)
            {
                return defaultValue;
            }
            
            return (T)Convert.ChangeType(setting.Name, type);
            
        }
        catch(Exception ex)
        {
            // TODO:MAYBE: log this?
            return defaultValue;
        }
    }
}