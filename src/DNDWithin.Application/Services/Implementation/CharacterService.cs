using System.Collections;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Repositories;
using FluentValidation;

namespace DNDWithin.Application.Services.Implementation;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IValidator<Character> _characterValidator;
    private readonly IValidator<GetAllCharactersOptions> _optionsValidator;

    public CharacterService(ICharacterRepository characterRepository, IValidator<Character> characterValidator, IValidator<GetAllCharactersOptions> optionsValidator)
    {
        _characterRepository = characterRepository;
        _characterValidator = characterValidator;
        _optionsValidator = optionsValidator;
    }

    public async Task<bool> CreateAsync(Character character, CancellationToken token = default)
    {
        await _characterValidator.ValidateAndThrowAsync(character, token);
        
        bool result = await _characterRepository.CreateAsync(character, token);

        return result;
    }

    public async Task<Character?> GetAsync(Guid id, CancellationToken token = default)
    {
        Character? result = await _characterRepository.GetByIdAsync(id, token);

        return result;
    }

    public async Task<IEnumerable<Character>> GetAllAsync(GetAllCharactersOptions options, CancellationToken token = default)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, token);
        
        IEnumerable<Character> results = await _characterRepository.GetAllAsync(options, token);

        return results;
    }

    public async Task<int> GetCountAsync(GetAllCharactersOptions options, CancellationToken token = default)
    {
        int result = await _characterRepository.GetCountAsync(options, token);

        return result;
    }

    public async Task<bool> UpdateAsync(Character character, CancellationToken token = default)
    {
        bool exists = await _characterRepository.ExistsById(character.Id, token);

        if (!exists)
        {
            return false; // not found
        }

        bool result = await _characterRepository.UpdateAsync(character, token);

        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        var exists = await _characterRepository.ExistsById(id, token);

        if (!exists)
        {
            return false; // not found
        }

        bool result = await _characterRepository.DeleteAsync(id, token);

        return result;
    }
}