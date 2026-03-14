namespace DCServerCore.Auth;

public interface ITokenService
{
    string GenerateAccessToken(int userId);
    DateTime GetAccessTokenExpiry();
    Task<string> GenerateRefreshTokenAsync(int userId, CancellationToken cancellationToken = default);
    Task<int?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
