using DNDWithin.Application.Models.Accounts;
using FluentValidation;

namespace DNDWithin.Application.Validators.Accounts;

public class GetAllAccountsOptionsValidator : AbstractValidator<GetAllAccountsOptions>
{
    public static readonly string[] AcceptableSortFields =
    [
        "username",
        "last_login_utc"
    ];

    public GetAllAccountsOptionsValidator()
    {
        RuleFor(x => x.SortField).Must(x => x is null || AcceptableSortFields.Contains(x)).WithMessage("You can only sort by Username or Lastlogin");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 25).WithMessage("You can get between 1 and 25 accounts per page");
    }
}