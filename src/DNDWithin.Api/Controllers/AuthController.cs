using DNDWithin.Api.Services;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DNDWithin.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGeneratorService _jwtTokenGeneratorService;

    public AuthController(IAccountService accountService, IPasswordHasher passwordHasher, IJwtTokenGeneratorService jwtTokenGeneratorService)
    {
        _accountService = accountService;
        _passwordHasher = passwordHasher;
        _jwtTokenGeneratorService = jwtTokenGeneratorService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken token)
    {
        Account? account;
        if (request.Email.Contains('@'))
        {
            account = await _accountService.GetByEmailAsync(request.Email, token);
        }
        else
        {
            account = await _accountService.GetByUsernameAsync(request.Email, token);
        }

        if (account is null)
        {
            return NotFound();
        }

        bool verified = _passwordHasher.Verify(request.Password, account.Password);

        if (!verified)
        {
            return Unauthorized();
        }

        string jwtToken = _jwtTokenGeneratorService.GenerateToken(account, request, token);

        return Ok(jwtToken);
    }
}