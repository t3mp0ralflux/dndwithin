using Asp.Versioning;
using DNDWithin.Api.Auth;
using DNDWithin.Api.Mapping;
using DNDWithin.Application.Models;
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

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Accounts.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
    {
        return Ok("Deleted");
    }
}