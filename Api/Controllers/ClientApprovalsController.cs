using System.Security.Claims;
using Application.Features.ClientApprovals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/client")]
[Authorize(Roles = "Client")]
public class ClientApprovalsController : BaseApiController
{
    private readonly IClientApprovalService _clientApprovalService;

    public ClientApprovalsController(IClientApprovalService clientApprovalService)
    {
        _clientApprovalService = clientApprovalService;
    }

    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientApprovalService.GetPendingApprovalsAsync(currentPersonId, cancellationToken);
        return FromResult(result, approvals => Ok(approvals));
    }

    [HttpPost("order-services/{orderServiceId:int}/approve")]
    public async Task<IActionResult> ApproveOrderService(int orderServiceId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientApprovalService.ApproveOrderServiceAsync(orderServiceId, currentPersonId, currentUserId, cancellationToken);
        return FromResult(result, approval => Ok(approval));
    }

    [HttpPost("order-services/{orderServiceId:int}/reject")]
    public async Task<IActionResult> RejectOrderService(int orderServiceId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientApprovalService.RejectOrderServiceAsync(orderServiceId, currentPersonId, currentUserId, cancellationToken);
        return FromResult(result, approval => Ok(approval));
    }

    [HttpPost("order-service-parts/{orderServicePartId:int}/approve")]
    public async Task<IActionResult> ApproveOrderServicePart(int orderServicePartId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientApprovalService.ApproveOrderServicePartAsync(orderServicePartId, currentPersonId, currentUserId, cancellationToken);
        return FromResult(result, approval => Ok(approval));
    }

    [HttpPost("order-service-parts/{orderServicePartId:int}/reject")]
    public async Task<IActionResult> RejectOrderServicePart(int orderServicePartId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientApprovalService.RejectOrderServicePartAsync(orderServicePartId, currentPersonId, currentUserId, cancellationToken);
        return FromResult(result, approval => Ok(approval));
    }

    private bool TryGetCurrentPersonId(out int personId)
    {
        personId = 0;

        var personIdClaim = User.FindFirstValue("personId");
        return int.TryParse(personIdClaim, out personId) && personId > 0;
    }

    private bool TryGetCurrentContext(out int userId, out int personId)
    {
        userId = 0;
        personId = 0;

        var userIdClaim = User.FindFirstValue("userId");
        if (!int.TryParse(userIdClaim, out userId) || userId <= 0)
        {
            return false;
        }

        var personIdClaim = User.FindFirstValue("personId");
        return int.TryParse(personIdClaim, out personId) && personId > 0;
    }
}
