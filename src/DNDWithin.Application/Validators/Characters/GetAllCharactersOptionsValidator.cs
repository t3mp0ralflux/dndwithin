using DNDWithin.Application.Models.Characters;
using FluentValidation;

namespace DNDWithin.Application.Validators.Characters;

public class GetAllCharactersOptionsValidator : AbstractValidator<GetAllCharactersOptions>
{
    public GetAllCharactersOptionsValidator()
    {
        
    }
}