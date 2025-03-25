using System.Security.Claims;

namespace DNDWithin.Api.Auth;

public static class IdentityExtenstions
{
    public static string? GetUserEmail(this HttpContext context)
    {
        Claim? email = context.User.Claims.SingleOrDefault(x => x.Type == "email");

        return email?.Value;
    }
}