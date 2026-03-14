using DCData.Entities;
using DCData.Repositories.Auth;
using DCData.Security;
using DCData.Session;
using DCProtocol.Auth;

namespace DCServerCore.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionStore _sessionStore;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISessionStore sessionStore)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _sessionStore = sessionStore;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (user == null)
            return null;

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        var sessionId = await _sessionStore.CreateAsync(user.Id, cancellationToken: cancellationToken);

        return new LoginResponse
        {
            SessionId = sessionId,
            UserId = user.Id,
            Nickname = user.Nickname
        };
    }

    public async Task<SignupResult> SignupAsync(string email, string password, string nickname, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (existing != null)
            return new SignupResult { Success = false, DuplicateEmail = true };

        var passwordHash = _passwordHasher.Hash(password);
        var now = DateTime.UtcNow;

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Nickname = nickname,
            Level = 1,
            Gold = 0,
            Stamina = 100,
            LastLoginAt = now,
            CreatedAt = now
        };

        var userId = await _userRepository.CreateAsync(user, cancellationToken);
        var sessionId = await _sessionStore.CreateAsync(userId, cancellationToken: cancellationToken);

        return new SignupResult
        {
            Success = true,
            Data = new LoginResponse
            {
                SessionId = sessionId,
                UserId = userId,
                Nickname = nickname
            }
        };
    }
}
