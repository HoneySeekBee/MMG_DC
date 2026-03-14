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

    public async Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var factory = await _queryFactoryProvider.CreateAsync();
        using (factory)
        {
            return await factory.Query("users")
                .Where("Id", id)
                .FirstOrDefaultAsync<User>();
        }
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

    public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var factory = await _queryFactoryProvider.CreateAsync();
        using (factory)
        {
            var id = await factory.Query("users").InsertGetIdAsync<int>(new
            {
                user.Email,
                user.PasswordHash,
                user.Nickname,
                user.Level,
                user.Gold,
                user.Stamina,
                user.CurrentDungeonId,
                user.CurrentFloor,
                user.LastLoginAt,
                user.CreatedAt,
                user.UpdatedAt
            });
            return id;
        }
    }
}
