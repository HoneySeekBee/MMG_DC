using DCData.Entities;

namespace DCData.Repositories.Auth;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(User user, CancellationToken cancellationToken = default);
}
