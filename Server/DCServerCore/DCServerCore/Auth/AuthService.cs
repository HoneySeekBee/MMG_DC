using DCData.Entities;
using DCData.Repositories.Auth;
using DCData.Security;
using DCProtocol.Auth;

namespace DCServerCore.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (user == null)
            return null;

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        var accessToken = _tokenService.GenerateAccessToken(user.Id);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _tokenService.GetAccessTokenExpiry(),
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
        var accessToken = _tokenService.GenerateAccessToken(userId);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, cancellationToken);

        return new SignupResult
        {
            Success = true,
            Data = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiry(),
                UserId = userId,
                Nickname = nickname
            }
        };
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = await _tokenService.ValidateRefreshTokenAsync(refreshToken, cancellationToken);
        if (userId == null)
            return null;

        var user = await _userRepository.FindByIdAsync(userId.Value, cancellationToken);
        if (user == null)
            return null;

        await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _tokenService.GetAccessTokenExpiry(),
            UserId = user.Id,
            Nickname = user.Nickname
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = await _tokenService.ValidateRefreshTokenAsync(refreshToken, cancellationToken);
        if (userId == null)
            return false;

        await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        return true;
    }
}
