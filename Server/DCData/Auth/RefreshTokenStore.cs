using StackExchange.Redis;

namespace DCData.Auth;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private const string KeyPrefix = "refresh:";

    private readonly IConnectionMultiplexer _redis;

    public RefreshTokenStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task StoreAsync(string refreshTokenId, int userId, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + refreshTokenId;
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, userId.ToString(), expiry);
    }

    public async Task<int?> GetUserIdAsync(string refreshTokenId, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + refreshTokenId;
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;

        return int.TryParse(value!, out var userId) ? userId : null;
    }

    public async Task RemoveAsync(string refreshTokenId, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + refreshTokenId;
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
