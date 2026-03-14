namespace DCData.Auth;

public interface IRefreshTokenStore
{
    Task StoreAsync(string refreshTokenId, int userId, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<int?> GetUserIdAsync(string refreshTokenId, CancellationToken cancellationToken = default);
    Task RemoveAsync(string refreshTokenId, CancellationToken cancellationToken = default);
}
