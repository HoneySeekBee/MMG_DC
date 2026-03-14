namespace DCData.Session;

public interface ISessionStore
{
    Task<string> CreateAsync(int userId, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<SessionData?> GetAsync(string sessionId, CancellationToken cancellationToken = default);
    Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default);
}

public record SessionData(int UserId);
