using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Repositories;

namespace DNDWithin.Application.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    
    public AuthService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }
    
    public async Task<bool> LoginAsync(AccountLogin accountLogin, CancellationToken token = default)
    {
        return await _accountRepository.LoginAsync(accountLogin, token);
    }
}