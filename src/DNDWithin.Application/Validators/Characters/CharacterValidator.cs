using DNDWithin.Application.Models.Characters;
using FluentValidation;

namespace DNDWithin.Application.Validators.Characters;

public class CharacterValidator : AbstractValidator<Character>
{
    public CharacterValidator()
    {
        
    }
}