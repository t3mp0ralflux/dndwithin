using DNDWithin.Api.Auth;
using DNDWithin.Api.Mapping;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Services;
using DNDWithin.Contracts.Requests.Characters;
using DNDWithin.Contracts.Responses.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNDWithin.Api.Controllers;

[ApiController]
[Authorize]
public class CharacterController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ICharacterService _characterService;

    public CharacterController(IAccountService accountService, ICharacterService characterService)
    {
        _accountService = accountService;
        _characterService = characterService;
    }

    [HttpPost(ApiEndpoints.Characters.Create)]
    public async Task<IActionResult> Create(CancellationToken token)
    {
        Account? account = await _accountService.GetByEmailAsync(HttpContext.GetUserEmail()!, token);

        if (account is null)
        {
            return Unauthorized();
        }

        Character character = new()
                              {
                                  Id = Guid.NewGuid(),
                                  AccountId = account.Id,
                                  Username = account.Username,
                                  Name = $"{account.Username}'s Unnamed Character"
                              };

        await _characterService.CreateAsync(character, token);

        CharacterResponse response = character.ToResponse();

        return CreatedAtAction(nameof(Get), new { id = character.Id }, response);
    }

    [HttpGet(ApiEndpoints.Characters.Get)]
    public async Task<IActionResult> Get([FromRoute]Guid id, CancellationToken token)
    {
        Account? account = await _accountService.GetByEmailAsync(HttpContext.GetUserEmail()!, token);

        if (account is null)
        {
            return Unauthorized();
        }

        Character? character = await _characterService.GetAsync(id, token);

        if (character is null)
        {
            return NotFound();
        }

        CharacterResponse response = character.ToResponse();

        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Characters.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery]GetAllCharactersRequest request, CancellationToken token)
    {
        Account? account = await _accountService.GetByEmailAsync(HttpContext.GetUserEmail()!, token);

        if (account is null)
        {
            return Unauthorized();
        }

        GetAllCharactersOptions options = request.ToOptions(account.Id);

        IEnumerable<Character> characters = await _characterService.GetAllAsync(options, token);
        int characterCount = await _characterService.GetCountAsync(options, token);

        CharactersResponse response = characters.ToGetAllResponse(request.Page, request.PageSize, characterCount);

        return Ok(response);
    }

    [HttpPut(ApiEndpoints.Characters.Update)]
    public async Task<IActionResult> Update([FromBody]CharacterUpdateRequest request, CancellationToken token)
    {
        Account? account = await _accountService.GetByEmailAsync(HttpContext.GetUserEmail()!, token);

        if (account is null)
        {
            return Unauthorized();
        }

        Character character = request.ToCharacter(account);

        bool result = await _characterService.UpdateAsync(character, token);

        if (!result)
        {
            return NotFound();
        }

        CharacterResponse response = character.ToResponse();

        return Ok(response);
    }

    [HttpDelete(ApiEndpoints.Characters.Delete)]
    public async Task<IActionResult> Delete([FromRoute]Guid id, CancellationToken token)
    {
        Account? account = await _accountService.GetByEmailAsync(HttpContext.GetUserEmail()!, token);

        if (account is null)
        {
            return Unauthorized();
        }

        bool result = await _characterService.DeleteAsync(id, token);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}