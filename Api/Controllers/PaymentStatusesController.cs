using Application.Features.PaymentStatuses;
using Application.Features.PaymentStatuses.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/payment-statuses")]
[Authorize(Roles = "Admin")]
public class PaymentStatusesController : BaseApiController
{
    private readonly IPaymentStatusService _paymentStatusService;

    public PaymentStatusesController(IPaymentStatusService paymentStatusService)
    {
        _paymentStatusService = paymentStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentStatusService.GetAllAsync(cancellationToken);
        return FromResult(result, paymentStatuses => Ok(paymentStatuses));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentStatusService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, paymentStatus => Ok(paymentStatus));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentStatusService.CreateAsync(request, cancellationToken);
        return FromResult(result, paymentStatus => CreatedAtAction(nameof(GetById), new { id = paymentStatus.PaymentStatusId }, paymentStatus));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePaymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentStatusService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, paymentStatus => Ok(paymentStatus));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentStatusService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
