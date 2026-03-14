using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DCData.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DCServerCore.Auth;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public TokenService(IConfiguration configuration, IRefreshTokenStore refreshTokenStore)
    {
        _configuration = configuration;
        _refreshTokenStore = refreshTokenStore;
    }

    public string GenerateAccessToken(int userId)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is required");
        var issuer = _configuration["Jwt:Issuer"] ?? "MMG_DC";
        var audience = _configuration["Jwt:Audience"] ?? "MMG_DC";
        var expiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetAccessTokenExpiry()
    {
        var expiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public async Task<string> GenerateRefreshTokenAsync(int userId, CancellationToken cancellationToken = default)
    {
        var refreshTokenId = Guid.NewGuid().ToString("N");
        var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
        var expiry = TimeSpan.FromDays(expiryDays);

        await _refreshTokenStore.StoreAsync(refreshTokenId, userId, expiry, cancellationToken);

        return refreshTokenId;
    }

    public async Task<int?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        return await _refreshTokenStore.GetUserIdAsync(refreshToken, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        await _refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);
    }
}
