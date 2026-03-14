namespace DCProtocol.Auth;

public class SignupResult
{
    public bool Success { get; set; }
    public LoginResponse? Data { get; set; }
    public bool DuplicateEmail { get; set; }
}
