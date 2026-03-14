using StackExchange.Redis;

namespace DCData.Session;

public sealed class SessionStore : ISessionStore
{
    private const string KeyPrefix = "session:";
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(24);

    private readonly IConnectionMultiplexer _redis;

    public SessionStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string> CreateAsync(int userId, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var key = KeyPrefix + sessionId;
        var expiryTime = expiry ?? DefaultExpiry;

        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, userId.ToString(), expiryTime);

        return sessionId;
    }

    public async Task<SessionData?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + sessionId;
        var db = _redis.GetDatabase();

        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;

        if (!int.TryParse(value!, out var userId))
            return null;

        return new SessionData(userId);
    }

    public async Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + sessionId;
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
