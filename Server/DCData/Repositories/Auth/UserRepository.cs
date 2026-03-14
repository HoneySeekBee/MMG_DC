using DCData.Entities;
using DCData.Querying;
using SqlKata.Execution;

namespace DCData.Repositories.Auth;

public sealed class UserRepository : IUserRepository
{
    private readonly IQueryFactoryProvider _queryFactoryProvider;

    public UserRepository(IQueryFactoryProvider queryFactoryProvider)
    {
        _queryFactoryProvider = queryFactoryProvider;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var factory = await _queryFactoryProvider.CreateAsync();
        using (factory)
        {
            return await factory.Query("users")
                .Where("Email", email)
                .FirstOrDefaultAsync<User>();
        }
    }
}
