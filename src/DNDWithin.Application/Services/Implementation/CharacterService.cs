using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;

namespace DNDWithin.Application.Services.Implementation;

public class CharacterService : ICharacterService
{
    public Task<Character> CreateAsync(Account account, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<Character?> GetAsync(Guid accountId, Guid id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Character>> GetAllAsync(GetAllCharactersOptions options, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync(GetAllCharactersOptions options, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Character character, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid accountId, Guid id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}