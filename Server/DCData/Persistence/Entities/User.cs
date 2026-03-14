namespace DCData.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public int Level { get; set; } = 1;
    public int Gold { get; set; } = 0;
    public int Stamina { get; set; } = 100;
    public int? CurrentDungeonId { get; set; }
    public int? CurrentFloor { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}