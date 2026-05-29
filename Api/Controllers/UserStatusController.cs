using System.Security.Claims;
using Application.Features.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UserStatusController : BaseApiController
{
    private readonly IStaffService _staffService;

    public UserStatusController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        var result = await _staffService.ActivateUserAsync(id, cancellationToken);
        return FromResult(result, user => Ok(user));
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var currentUserIdClaim = User.FindFirstValue("userId");
        if (!int.TryParse(currentUserIdClaim, out var currentUserId) || currentUserId <= 0)
        {
            return Unauthorized();
        }

        var result = await _staffService.DeactivateUserAsync(id, currentUserId, cancellationToken);
        return FromResult(result, user => Ok(user));
    }
}
