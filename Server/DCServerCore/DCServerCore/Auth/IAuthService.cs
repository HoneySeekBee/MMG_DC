using DCProtocol.Auth;

namespace DCServerCore.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
