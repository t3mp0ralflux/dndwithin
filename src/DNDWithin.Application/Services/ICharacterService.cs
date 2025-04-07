using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;

namespace DNDWithin.Application.Services;

public interface ICharacterService
{
    Task<Character> CreateAsync(Account account, CancellationToken token = default);
    Task<Character?> GetAsync(Guid accountId, Guid id, CancellationToken token = default);
    Task<IEnumerable<Character>> GetAllAsync(GetAllCharactersOptions options, CancellationToken token = default);
    Task<int> GetCountAsync(GetAllCharactersOptions options, CancellationToken token = default);
    Task<bool> UpdateAsync(Character character, CancellationToken token = default);
    Task<bool> DeleteAsync(Guid accountId, Guid id, CancellationToken token = default);
}
