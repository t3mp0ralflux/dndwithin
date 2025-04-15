using DNDWithin.Api.Mapping;
using DNDWithin.Api.Services;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Services;
using DNDWithin.Contracts.Requests.Auth;
using DNDWithin.Contracts.Responses.Auth;
using DNDWithin.Contracts.Responses.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DNDWithin.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAuthService _authService;
    private readonly IJwtTokenGeneratorService _jwtTokenGeneratorService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(IAccountService accountService, IPasswordHasher passwordHasher, IJwtTokenGeneratorService jwtTokenGeneratorService, IAuthService authService)
    {
        _accountService = accountService;
        _passwordHasher = passwordHasher;
        _jwtTokenGeneratorService = jwtTokenGeneratorService;
        _authService = authService;
    }

    [HttpPost(ApiEndpoints.Auth.Login)]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<UnauthorizedObjectResult>(StatusCodes.Status401Unauthorized)]
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

        if (account.AccountStatus != AccountStatus.active)
        {
            return Unauthorized("You must activate your account before you can login");
        }
        
        bool verified = _passwordHasher.Verify(request.Password, account.Password);

        if (!verified)
        {
            return Unauthorized("Username or password is incorrect");
        }

        AccountLogin accountLogin = account.ToLogin();

        await _authService.LoginAsync(accountLogin, token);

        string jwtToken = _jwtTokenGeneratorService.GenerateToken(account, token);

        return Ok(jwtToken);
    }

    [HttpPost(ApiEndpoints.Auth.RequestPasswordReset)]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPasswordReset([FromRoute] string email, CancellationToken token)
    {
        await _accountService.RequestPasswordReset(email, token); // DO NOT SURFACE TO USER. If the account isn't found, it'll fail in silence.

        return Ok(email);
    }

    [HttpPost(ApiEndpoints.Auth.VerifyPasswordResetCode)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationFailureResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyPasswordResetCode([FromRoute]string email, [FromBody] PasswordResetVerification verification, CancellationToken token)
    {
        if (!string.Equals(email.ToLowerInvariant(), verification.Email.ToLowerInvariant()))
        {
            throw new ValidationException("Email not valid");
        }
        
        // throws ValidationExceptions if not valid
        await _accountService.VerifyPasswordResetCode(verification.Email, verification.Code, token);
        
        return Ok();
    }

    [HttpPost(ApiEndpoints.Auth.PasswordReset)]
    [ProducesResponseType<PasswordResetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationFailureResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PasswordReset([FromBody] PasswordResetRequest request, CancellationToken token)
    {
        bool accountExists = await _accountService.ExistsByEmailAsync(request.Email, token);

        if (!accountExists)
        {
            return NotFound();
        }

        PasswordReset passwordReset = request.ToReset();

        await _accountService.ResetPassword(passwordReset, token);

        PasswordResetResponse response = passwordReset.ToResponse();

        return Ok(response);
    }
}