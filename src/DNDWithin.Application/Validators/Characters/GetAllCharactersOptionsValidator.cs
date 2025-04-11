using DNDWithin.Application.Models.Characters;
using FluentValidation;

namespace DNDWithin.Application.Validators.Characters;

public class GetAllCharactersOptionsValidator : AbstractValidator<GetAllCharactersOptions>
{
    public static readonly string[] AcceptableSortFields = 
    [
        "name",
        "level",
        "species",
        "class"
    ];
    
    public GetAllCharactersOptionsValidator()
    {
        RuleFor(x => x.AccountId).NotNull().NotEqual(Guid.Empty);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 25).WithMessage("You can get between 1 and 25 characters per page");
        RuleFor(x => x.SortField).Must(x => x is null || AcceptableSortFields.Contains(x)).WithMessage("You can only sort by name, level, species, or class");
    }
}