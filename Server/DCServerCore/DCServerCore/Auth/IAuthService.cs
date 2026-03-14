using DCProtocol.Auth;

namespace DCServerCore.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<SignupResult> SignupAsync(string email, string password, string nickname, CancellationToken cancellationToken = default);
    Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
