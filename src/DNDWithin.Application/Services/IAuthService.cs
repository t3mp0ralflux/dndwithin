using DNDWithin.Application.Models.Auth;

namespace DNDWithin.Application.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(AccountLogin accountLogin, CancellationToken token = default);
}
