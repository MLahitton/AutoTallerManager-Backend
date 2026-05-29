using Application.Features.PartPurchaseDetails;
using Application.Features.PartPurchaseDetails.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/part-purchase-details")]
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
        var result = await _partPurchaseDetailService.CreateAsync(request, cancellationToken);
        return FromResult(result, partPurchaseDetail => CreatedAtAction(nameof(GetById), new { id = partPurchaseDetail.PartPurchaseDetailId }, partPurchaseDetail));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartPurchaseDetailRequest request, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseDetailService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, partPurchaseDetail => Ok(partPurchaseDetail));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseDetailService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
