using System.Security.Claims;
using Application.Features.InventoryBusiness;
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
        var result = await _inventoryBusinessService.RegisterPurchaseAsync(request, cancellationToken);
        return FromResult(result, purchase => Ok(purchase));
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
}
