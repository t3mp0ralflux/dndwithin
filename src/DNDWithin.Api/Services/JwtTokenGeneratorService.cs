using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DNDWithin.Api.Auth;
using DNDWithin.Application;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;

namespace DNDWithin.Api.Services;

public interface IJwtTokenGeneratorService
{
    string GenerateToken(Account account, LoginRequest request, CancellationToken token = default);
}

public class JwtTokenGeneratorService : IJwtTokenGeneratorService
{
    private readonly IConfiguration _config;
    private readonly TimeSpan TokenLifetime;

    public JwtTokenGeneratorService(IConfiguration configuration, IGlobalSettingsService globalSettingsService)
    {
        _config = configuration;

        int lifetimeHours = globalSettingsService.GetSettingAsync(WellKnownGlobalSettings.JWT_TOKEN_SECRET, 8).Result;
        TokenLifetime = TimeSpan.FromHours(lifetimeHours);
    }

    public string GenerateToken(Account account, LoginRequest request, CancellationToken token = default)
    {
        string? tokenSecret = _config["Jwt:Key"];
        JwtSecurityTokenHandler tokenHandler = new();

        byte[] key = Encoding.UTF8.GetBytes(tokenSecret);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, request.Email),
            new(JwtRegisteredClaimNames.Email, request.Email),
            new(AuthConstants.AdminUserClaimName, (account.AccountRole == AccountRole.admin).ToString().ToLower()),
            new(AuthConstants.TrustedUserClaimName, (account.AccountRole is AccountRole.admin or AccountRole.trusted).ToString().ToLower())
        ];

        SecurityTokenDescriptor tokenDescriptor = new()
                                                  {
                                                      Subject = new ClaimsIdentity(claims),
                                                      Expires = DateTime.UtcNow.Add(TokenLifetime),
                                                      Issuer = _config["Jwt:Issuer"],
                                                      Audience = _config["Jwt:Audience"],
                                                      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                                  };

        SecurityToken newToken = tokenHandler.CreateToken(tokenDescriptor);

        string jwt = tokenHandler.WriteToken(newToken);

        return jwt;
    }
}