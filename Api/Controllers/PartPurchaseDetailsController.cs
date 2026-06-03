using System.Security.Claims;
using Application.Features.PartPurchaseDetails;
using Application.Features.PartPurchaseDetails.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/part-purchase-details")]
[Authorize(Roles = "Admin,Receptionist")]
public class PartPurchaseDetailsController : BaseApiController
{
    private readonly IPartPurchaseDetailService _partPurchaseDetailService;

    public PartPurchaseDetailsController(IPartPurchaseDetailService partPurchaseDetailService)
    {
        _partPurchaseDetailService = partPurchaseDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _partPurchaseDetailService.GetAllAsync(cancellationToken);
        return FromResult(result, partPurchaseDetails => Ok(partPurchaseDetails));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseDetailService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, partPurchaseDetail => Ok(partPurchaseDetail));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartPurchaseDetailRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _partPurchaseDetailService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, partPurchaseDetail => CreatedAtAction(nameof(GetById), new { id = partPurchaseDetail.PartPurchaseDetailId }, partPurchaseDetail));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartPurchaseDetailRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _partPurchaseDetailService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, partPurchaseDetail => Ok(partPurchaseDetail));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _partPurchaseDetailService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
