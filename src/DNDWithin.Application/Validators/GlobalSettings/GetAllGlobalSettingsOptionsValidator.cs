using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.GlobalSettings;
using FluentValidation;

namespace DNDWithin.Application.Validators.GlobalSettings;

public class GetAllGlobalSettingsOptionsValidator : AbstractValidator<GetAllGlobalSettingsOptions>
{
    public static readonly string[] AcceptableSortFields =
    [
        "name"
    ];

    public GetAllGlobalSettingsOptionsValidator()
    {
        RuleFor(x => x.SortField).Must(x => x is null || AcceptableSortFields.Contains(x)).WithMessage("You can only sort by Name");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 25).WithMessage("You can get between 1 and 25 settings per page");
    }
}