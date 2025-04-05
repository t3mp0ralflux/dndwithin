using Asp.Versioning;
using DNDWithin.Api.Auth;
using DNDWithin.Api.Mapping;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using DNDWithin.Contracts.Requests.Account;
using DNDWithin.Contracts.Responses.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNDWithin.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost(ApiEndpoints.Accounts.Create)]
    public async Task<IActionResult> Create([FromBody] AccountCreateRequest request, CancellationToken token)
    {
        Account account = request.ToAccount();
        await _accountService.CreateAsync(account, token);

        AccountResponse response = account.ToResponse();

        return CreatedAtAction(nameof(Get), new { id = account.Id }, response);
    }

    [HttpGet(ApiEndpoints.Accounts.Get)]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken token)
    {
        Account? account = await _accountService.GetByIdAsync(id, token);

        if (account is null)
        {
            return NotFound();
        }

        AccountResponse response = account.ToResponse();

        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Accounts.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllAccountsRequest request, CancellationToken token)
    {
        GetAllAccountsOptions options = request.ToOptions();
        IEnumerable<Account> result = await _accountService.GetAllAsync(options, token);
        int accountCount = await _accountService.GetCountAsync(options.UserName, token);

        AccountsResponse response = result.ToResponse(request.Page, request.PageSize, accountCount);

        return Ok(response);
    }

    [HttpPut(ApiEndpoints.Accounts.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] AccountUpdateRequest request, CancellationToken token)
    {
        Account account = request.ToAccount(id);
        Account? result = await _accountService.UpdateAsync(account, token);

        if (result is null)
        {
            return NotFound();
        }

        AccountResponse response = account.ToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Accounts.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
    {
        bool deleted = await _accountService.DeleteAsync(id, token);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet(ApiEndpoints.Accounts.Activate)]
    public async Task<IActionResult> Activate([FromRoute] string username, [FromRoute] string activationcode, CancellationToken token)
    {
        AccountActivation activationRequest = new AccountActivation()
                                              {
                                                  ActivationCode = activationcode,
                                                  Username = username,
                                              };
        
        (bool isActive, string reason) activationResult = await _accountService.ActivateAsync(activationRequest, token);

        if (!activationResult.isActive)
        {
            return Unauthorized(activationResult.reason);
        }

        AccountActivationResponse response = activationRequest.ToResponse();

        return Ok(response);
    }

    [HttpPost(ApiEndpoints.Accounts.ResendActivation)]
    public async Task<IActionResult> ResendActivation([FromRoute] string username, [FromRoute] string activationCode, CancellationToken token)
    {
        var resendActivationResult = await _accountService.ResendActivation(username, activationCode, token);

        return Ok();
    }
}