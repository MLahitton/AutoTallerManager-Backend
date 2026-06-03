using System.Security.Claims;
using Application.Features.ServiceExecution;
using Application.Features.ServiceExecution.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/order-services")]
public class OrderServiceBusinessController : BaseApiController
{
    private readonly IServiceExecutionService _serviceExecutionService;

    public OrderServiceBusinessController(IServiceExecutionService serviceExecutionService)
    {
        _serviceExecutionService = serviceExecutionService;
    }

    [HttpPut("{id:int}/work-report")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> UpdateWorkReport(
        int id,
        [FromBody] UpdateWorkPerformedRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.UpdateWorkPerformedAsync(
            id,
            currentPersonId,
            currentUserId,
            currentRoles,
            request,
            cancellationToken);

        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("{id:int}/assign-mechanic")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> AssignMechanic(
        int id,
        [FromBody] AssignMechanicRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.AssignMechanicAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("{id:int}/unassign-mechanic")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> UnassignMechanic(
        int id,
        [FromBody] UnassignMechanicRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.UnassignMechanicAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("{id:int}/request-part")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> RequestPart(
        int id,
        [FromBody] RequestOrderServicePartRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.RequestPartAsync(
            id,
            currentPersonId,
            currentUserId,
            currentRoles,
            request,
            cancellationToken);

        return FromResult(result, execution => Ok(execution));
    }

    private bool TryGetCurrentContext(out int userId, out int personId, out IReadOnlyList<string> roles)
    {
        userId = 0;
        personId = 0;
        roles = Array.Empty<string>();

        var userIdClaim = User.FindFirstValue("userId");
        if (!int.TryParse(userIdClaim, out userId) || userId <= 0)
        {
            return false;
        }

        var personIdClaim = User.FindFirstValue("personId");
        if (!int.TryParse(personIdClaim, out personId) || personId <= 0)
        {
            return false;
        }

        roles = User.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return true;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
