using DCProtocol.Auth;

namespace DCServerCore.User;

public interface IUserService
{
    Task<UserProfileResponse?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
}
