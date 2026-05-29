using System.Security.Claims;
using Application.Features.ServiceExecution;
using Application.Features.ServiceExecution.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/order-service-parts")]
public class OrderServicePartBusinessController : BaseApiController
{
    private readonly IServiceExecutionService _serviceExecutionService;

    public OrderServicePartBusinessController(IServiceExecutionService serviceExecutionService)
    {
        _serviceExecutionService = serviceExecutionService;
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.ApproveOrderServicePartAsync(
            id,
            currentPersonId,
            currentRoles,
            cancellationToken);

        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.RejectOrderServicePartAsync(
            id,
            currentPersonId,
            currentRoles,
            cancellationToken);

        return FromResult(result, execution => Ok(execution));
    }

    [HttpPut("{id:int}/change-quantity")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> ChangeQuantity(
        int id,
        [FromBody] ChangeOrderServicePartQuantityRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.ChangeOrderServicePartQuantityAsync(
            id,
            currentPersonId,
            currentRoles,
            request,
            cancellationToken);

        return FromResult(result, execution => Ok(execution));
    }

    private bool TryGetCurrentContext(out int personId, out IReadOnlyList<string> roles)
    {
        personId = 0;
        roles = Array.Empty<string>();

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
}
