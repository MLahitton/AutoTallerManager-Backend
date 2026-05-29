using System.Security.Claims;
using Application.Features.ServiceExecution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/client")]
[Authorize(Roles = "Client")]
public class ClientApprovalsController : BaseApiController
{
    private readonly IServiceExecutionService _serviceExecutionService;

    public ClientApprovalsController(IServiceExecutionService serviceExecutionService)
    {
        _serviceExecutionService = serviceExecutionService;
    }

    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.GetClientPendingApprovalsAsync(currentPersonId, cancellationToken);
        return FromResult(result, approvals => Ok(approvals));
    }

    [HttpPost("approvals/order-services/{id:int}/approve")]
    public async Task<IActionResult> ApproveOrderService(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.ApproveOrderServiceAsync(id, currentPersonId, cancellationToken);
        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("approvals/order-services/{id:int}/reject")]
    public async Task<IActionResult> RejectOrderService(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.RejectOrderServiceAsync(id, currentPersonId, cancellationToken);
        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("approvals/order-service-parts/{id:int}/approve")]
    public async Task<IActionResult> ApproveOrderServicePart(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.ClientApproveOrderServicePartAsync(id, currentPersonId, cancellationToken);
        return FromResult(result, execution => Ok(execution));
    }

    [HttpPost("approvals/order-service-parts/{id:int}/reject")]
    public async Task<IActionResult> RejectOrderServicePart(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _serviceExecutionService.ClientRejectOrderServicePartAsync(id, currentPersonId, cancellationToken);
        return FromResult(result, execution => Ok(execution));
    }

    private bool TryGetCurrentPersonId(out int personId)
    {
        personId = 0;

        var personIdClaim = User.FindFirstValue("personId");
        return int.TryParse(personIdClaim, out personId) && personId > 0;
    }
}
