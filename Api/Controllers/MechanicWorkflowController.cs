using System.Security.Claims;
using Application.Features.ServiceExecution;
using Application.Features.ServiceExecution.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/mechanic")]
[Authorize(Roles = "Mechanic")]
public class MechanicWorkflowController : BaseApiController
{
    private readonly IServiceExecutionService _serviceExecutionService;

    public MechanicWorkflowController(IServiceExecutionService serviceExecutionService)
    {
        _serviceExecutionService = serviceExecutionService;
    }

    [HttpGet("my-assigned-services")]
    public async Task<IActionResult> GetMyAssignedServices(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.GetMyAssignedServicesAsync(currentPersonId, cancellationToken);
        return FromResult(result, services => Ok(services));
    }

    [HttpGet("my-active-orders")]
    public async Task<IActionResult> GetMyActiveOrders(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out _))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.GetMyActiveOrdersAsync(currentPersonId, cancellationToken);
        return FromResult(result, orders => Ok(orders));
    }

    [HttpPut("order-services/{id:int}/work-performed")]
    public async Task<IActionResult> UpdateWorkPerformed(
        int id,
        [FromBody] UpdateWorkPerformedRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out var currentRoles))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.UpdateWorkPerformedAsync(
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
