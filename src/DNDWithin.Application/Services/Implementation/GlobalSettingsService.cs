using System.Security.Cryptography.X509Certificates;
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

    public Task<bool> CreateSettingAsync(string name, string value, CancellationToken token = default)
    {
        throw new NotImplementedException();
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
    
    public async Task<bool> GetSettingAsync(string name, bool defaultValue, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        bool success = bool.TryParse(setting?.Value, out bool result);

        return success ? result : defaultValue;
    }

    public async Task<Guid> GetSettingAsync(string name, Guid defaultValue, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        bool success = Guid.TryParse(setting?.Value, out Guid result);

        return success ? result : defaultValue;
    }

    public async Task<int> GetSettingAsync(string name, int defaultValue, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        bool success = int.TryParse(setting?.Value, out int result);

        return success ? result : defaultValue;
    }

    public async Task<double> GetSettingAsync(string name, double defaultValue, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        bool success = double.TryParse(setting?.Value, out double result);

        return success ? result : defaultValue;
    }

    public async Task<DateTime> GetSettingAsync(string name, DateTime defaultValue, CancellationToken token = default)
    {
        GlobalSetting? setting = await _globalSettingsRepository.GetSetting(name, token);

        bool success = DateTime.TryParse(setting?.Value, out DateTime result);

        return success ? result : defaultValue;
    }
}