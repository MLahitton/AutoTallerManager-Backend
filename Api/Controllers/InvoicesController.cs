using Application.Features.Invoices;
using Application.Features.Invoices.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Roles = "Admin,Receptionist")]
public class InvoicesController : BaseApiController
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetAllAsync(cancellationToken);
        return FromResult(result, invoices => Ok(invoices));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, invoice => Ok(invoice));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.CreateAsync(request, cancellationToken);
        return FromResult(result, invoice => CreatedAtAction(nameof(GetById), new { id = invoice.InvoiceId }, invoice));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, invoice => Ok(invoice));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
