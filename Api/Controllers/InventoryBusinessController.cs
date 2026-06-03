using System.Security.Claims;
using Application.Common.Results;
using Application.Features.InventoryBusiness;
using Application.Features.InventoryBusiness.Dtos;
using Application.Features.InventoryBusiness.Errors;
using Application.Features.InventoryBusiness.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryBusinessController : BaseApiController
{
    private readonly IInventoryBusinessService _inventoryBusinessService;

    public InventoryBusinessController(IInventoryBusinessService inventoryBusinessService)
    {
        _inventoryBusinessService = inventoryBusinessService;
    }

    [HttpPost("register-purchase")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> RegisterPurchase(
        [FromBody] RegisterInventoryPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            var invalidUserResult = Result<InventoryPurchaseResultDto>.Failure(InventoryBusinessErrors.CurrentUserInvalid);
            return FromResult(invalidUserResult, purchase => Ok(purchase));
        }

        var result = await _inventoryBusinessService.RegisterPurchaseAsync(request, currentUserId, cancellationToken);
        return FromResult(result, purchase => Ok(purchase));
    }

    [HttpPost("purchases/{purchaseId:int}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelPurchase(
        int purchaseId,
        [FromBody] CancelInventoryPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            var invalidUserResult = Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.CurrentUserInvalid);
            return FromResult(invalidUserResult, cancellation => Ok(cancellation));
        }

        var result = await _inventoryBusinessService.CancelPurchaseAsync(
            purchaseId,
            request,
            currentUserId,
            cancellationToken);

        return FromResult(result, cancellation => Ok(cancellation));
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetLowStock(CancellationToken cancellationToken)
    {
        var result = await _inventoryBusinessService.GetLowStockAsync(cancellationToken);
        return FromResult(result, parts => Ok(parts));
    }

    [HttpPost("adjust-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdjustStock(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue("userId");
        if (!int.TryParse(userIdClaim, out var changedByUserId) || changedByUserId <= 0)
        {
            return Unauthorized();
        }

        var result = await _inventoryBusinessService.AdjustStockAsync(changedByUserId, request, cancellationToken);
        return FromResult(result, adjustment => Ok(adjustment));
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _inventoryBusinessService.GetSummaryAsync(cancellationToken);
        return FromResult(result, summary => Ok(summary));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;

        var userIdClaim = User.FindFirstValue("userId");
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
