using DCProtocol.Auth;
using DCServerCore.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DCWebServer.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result == null)
            return Unauthorized();

        return Ok(result);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SignupAsync(request.Email, request.Password, request.Nickname, cancellationToken);
        if (result.DuplicateEmail)
            return Conflict("Email already exists");
        if (!result.Success || result.Data == null)
            return BadRequest();

        return Ok(result.Data);
    }
}
