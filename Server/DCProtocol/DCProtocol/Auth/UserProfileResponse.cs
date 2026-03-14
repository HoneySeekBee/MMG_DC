using System;

namespace DCProtocol.Auth;

public class UserProfileResponse
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Gold { get; set; }
    public int Stamina { get; set; }
    public int? CurrentDungeonId { get; set; }
    public int? CurrentFloor { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
