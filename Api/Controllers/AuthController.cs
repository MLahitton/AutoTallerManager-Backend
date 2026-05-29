using Application.Features.Auth;
using Application.Features.Auth.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register-client")]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterClientRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterClientAsync(request, cancellationToken);
        return FromResult(result, authResponse => Ok(authResponse));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return FromResult(result, authResponse => Ok(authResponse));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);
        return FromResult(result, authResponse => Ok(authResponse));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
