using Application.Features.InvoiceStatuses;
using Application.Features.InvoiceStatuses.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/invoice-statuses")]
[Authorize(Roles = "Admin")]
public class InvoiceStatusesController : BaseApiController
{
    private readonly IInvoiceStatusService _invoiceStatusService;

    public InvoiceStatusesController(IInvoiceStatusService invoiceStatusService)
    {
        _invoiceStatusService = invoiceStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _invoiceStatusService.GetAllAsync(cancellationToken);
        return FromResult(result, invoiceStatuses => Ok(invoiceStatuses));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceStatusService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, invoiceStatus => Ok(invoiceStatus));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _invoiceStatusService.CreateAsync(request, cancellationToken);
        return FromResult(result, invoiceStatus => CreatedAtAction(nameof(GetById), new { id = invoiceStatus.InvoiceStatusId }, invoiceStatus));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateInvoiceStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _invoiceStatusService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, invoiceStatus => Ok(invoiceStatus));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceStatusService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
