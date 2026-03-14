using DCData.Repositories.Auth;
using DCProtocol.Auth;

namespace DCServerCore.User;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponse?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
            return null;

        return new UserProfileResponse
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Level = user.Level,
            Gold = user.Gold,
            Stamina = user.Stamina,
            CurrentDungeonId = user.CurrentDungeonId,
            CurrentFloor = user.CurrentFloor,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
