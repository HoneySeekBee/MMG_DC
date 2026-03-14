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
}
