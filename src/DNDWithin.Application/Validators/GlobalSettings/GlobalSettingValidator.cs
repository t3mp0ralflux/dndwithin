using DNDWithin.Application.Models.GlobalSettings;
using FluentValidation;

namespace DNDWithin.Application.Validators.GlobalSettings;

public class GlobalSettingValidator : AbstractValidator<GlobalSetting>
{
    public GlobalSettingValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
    }
}