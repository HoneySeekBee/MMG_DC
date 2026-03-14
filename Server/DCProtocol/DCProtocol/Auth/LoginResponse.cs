namespace DCProtocol.Auth;

public class LoginResponse
{
    public string SessionId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
}