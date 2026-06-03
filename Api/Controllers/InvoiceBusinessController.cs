using System.Security.Claims;
using Application.Features.InvoiceBusiness;
using Application.Features.InvoiceBusiness.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoiceBusinessController : BaseApiController
{
    private readonly IInvoiceBusinessService _invoiceBusinessService;

    public InvoiceBusinessController(IInvoiceBusinessService invoiceBusinessService)
    {
        _invoiceBusinessService = invoiceBusinessService;
    }

    [HttpPost("generate-from-service-order/{serviceOrderId:int}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GenerateFromServiceOrder(
        int serviceOrderId,
        [FromBody] GenerateInvoiceFromServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invoiceBusinessService.GenerateFromServiceOrderAsync(serviceOrderId, request, currentUserId, cancellationToken);
        return FromResult(result, invoice => Ok(invoice));
    }

    [HttpPost("{id:int}/recalculate")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Recalculate(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invoiceBusinessService.RecalculateAsync(id, currentUserId, cancellationToken);
        return FromResult(result, invoice => Ok(invoice));
    }

    [HttpPost("{id:int}/issue")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Issue(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invoiceBusinessService.IssueAsync(id, currentUserId, cancellationToken);
        return FromResult(result, invoice => Ok(invoice));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(
        int id,
        [FromBody] CancelInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _invoiceBusinessService.CancelAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, invoice => Ok(invoice));
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
