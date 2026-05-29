using Application.Features.InvoiceDetails;
using Application.Features.InvoiceDetails.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/invoice-details")]
[Authorize(Roles = "Admin,Receptionist")]
public class InvoiceDetailsController : BaseApiController
{
    private readonly IInvoiceDetailService _invoiceDetailService;

    public InvoiceDetailsController(IInvoiceDetailService invoiceDetailService)
    {
        _invoiceDetailService = invoiceDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _invoiceDetailService.GetAllAsync(cancellationToken);
        return FromResult(result, invoiceDetails => Ok(invoiceDetails));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceDetailService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, invoiceDetail => Ok(invoiceDetail));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDetailRequest request, CancellationToken cancellationToken)
    {
        var result = await _invoiceDetailService.CreateAsync(request, cancellationToken);
        return FromResult(result, invoiceDetail => CreatedAtAction(nameof(GetById), new { id = invoiceDetail.InvoiceDetailId }, invoiceDetail));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceDetailRequest request, CancellationToken cancellationToken)
    {
        var result = await _invoiceDetailService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, invoiceDetail => Ok(invoiceDetail));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceDetailService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
