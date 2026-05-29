using System.Security.Claims;
using Application.Features.Account;
using Application.Features.Account.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedIds(out var userId, out var personId))
        {
            return Unauthorized();
        }

        var result = await _accountService.GetMeAsync(userId, personId, cancellationToken);
        return FromResult(result, profile => Ok(profile));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateAccountProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedIds(out var userId, out var personId))
        {
            return Unauthorized();
        }

        var result = await _accountService.UpdateMeAsync(userId, personId, request, cancellationToken);
        return FromResult(result, profile => Ok(profile));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedIds(out var userId, out _))
        {
            return Unauthorized();
        }

        var result = await _accountService.ChangePasswordAsync(userId, request, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetAuthenticatedIds(out int userId, out int personId)
    {
        userId = 0;
        personId = 0;

        var userIdClaim = User.FindFirstValue("userId");
        var personIdClaim = User.FindFirstValue("personId");

        if (!int.TryParse(userIdClaim, out userId))
        {
            return false;
        }

        if (!int.TryParse(personIdClaim, out personId))
        {
            return false;
        }

        return userId > 0 && personId > 0;
    }
}
