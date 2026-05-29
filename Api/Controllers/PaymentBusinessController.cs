using System.Security.Claims;
using Application.Features.PaymentBusiness;
using Application.Features.PaymentBusiness.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class PaymentBusinessController : BaseApiController
{
    private readonly IPaymentBusinessService _paymentBusinessService;

    public PaymentBusinessController(IPaymentBusinessService paymentBusinessService)
    {
        _paymentBusinessService = paymentBusinessService;
    }

    [HttpPost("invoices/{id:int}/record-payment")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> RecordPayment(
        int id,
        [FromBody] RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentBusinessService.RecordPaymentAsync(id, request, cancellationToken);
        return FromResult(result, payment => Ok(payment));
    }

    [HttpGet("invoices/{id:int}/payment-summary")]
    [Authorize(Roles = "Admin,Receptionist,Client")]
    public async Task<IActionResult> GetPaymentSummary(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentPersonId, out var roles))
        {
            return Unauthorized();
        }

        var result = await _paymentBusinessService.GetPaymentSummaryAsync(id, currentPersonId, roles, cancellationToken);
        return FromResult(result, summary => Ok(summary));
    }

    [HttpPost("payments/{id:int}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentBusinessService.RefundAsync(id, cancellationToken);
        return FromResult(result, payment => Ok(payment));
    }

    private bool TryGetCurrentContext(out int personId, out IReadOnlyList<string> roles)
    {
        personId = 0;
        roles = Array.Empty<string>();

        var personIdClaim = User.FindFirstValue("personId");
        if (!int.TryParse(personIdClaim, out personId) || personId <= 0)
        {
            return false;
        }

        roles = User.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return true;
    }
}
