using System.Security.Claims;
using Application.Features.ServiceOrderWorkflow;
using Application.Features.ServiceOrderWorkflow.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/service-orders")]
public class ServiceOrderWorkflowController : BaseApiController
{
    private readonly IServiceOrderWorkflowService _serviceOrderWorkflowService;

    public ServiceOrderWorkflowController(IServiceOrderWorkflowService serviceOrderWorkflowService)
    {
        _serviceOrderWorkflowService = serviceOrderWorkflowService;
    }

    [HttpGet("{id:int}/full-detail")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic,Client")]
    public async Task<IActionResult> GetFullDetail(int id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedContext(out var userId, out var personId, out var roles))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderWorkflowService.GetFullDetailAsync(
            id,
            userId,
            personId,
            roles,
            cancellationToken);

        return FromResult(result, detail => Ok(detail));
    }

    [HttpPost("{id:int}/change-status")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeServiceOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedContext(out var userId, out _, out _))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderWorkflowService.ChangeStatusAsync(
            id,
            userId,
            request,
            cancellationToken);

        return FromResult(result, workflow => Ok(workflow));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Cancel(
        int id,
        [FromBody] CancelOrVoidServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedContext(out var userId, out _, out _))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderWorkflowService.CancelAsync(
            id,
            userId,
            request,
            cancellationToken);

        return FromResult(result, workflow => Ok(workflow));
    }

    [HttpPost("{id:int}/void")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Void(
        int id,
        [FromBody] CancelOrVoidServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedContext(out var userId, out _, out _))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderWorkflowService.VoidAsync(
            id,
            userId,
            request,
            cancellationToken);

        return FromResult(result, workflow => Ok(workflow));
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedContext(out var userId, out _, out _))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderWorkflowService.CompleteAsync(id, userId, cancellationToken);
        return FromResult(result, workflow => Ok(workflow));
    }

    private bool TryGetAuthenticatedContext(out int userId, out int personId, out IReadOnlyList<string> roles)
    {
        userId = 0;
        personId = 0;
        roles = Array.Empty<string>();

        var userIdClaim = User.FindFirstValue("userId");
        var personIdClaim = User.FindFirstValue("personId");

        if (!int.TryParse(userIdClaim, out userId) || userId <= 0)
        {
            return false;
        }

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
