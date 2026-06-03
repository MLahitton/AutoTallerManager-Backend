using System.Security.Claims;
using Application.Features.PartPurchases;
using Application.Features.PartPurchases.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/part-purchases")]
[Authorize(Roles = "Admin,Receptionist")]
public class PartPurchasesController : BaseApiController
{
    private readonly IPartPurchaseService _partPurchaseService;

    public PartPurchasesController(IPartPurchaseService partPurchaseService)
    {
        _partPurchaseService = partPurchaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.GetAllAsync(cancellationToken);
        return FromResult(result, partPurchases => Ok(partPurchases));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, partPurchase => Ok(partPurchase));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartPurchaseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _partPurchaseService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, partPurchase => CreatedAtAction(nameof(GetById), new { id = partPurchase.PartPurchaseId }, partPurchase));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartPurchaseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _partPurchaseService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, partPurchase => Ok(partPurchase));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _partPurchaseService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
