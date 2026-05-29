using Application.Features.Payments;
using Application.Features.Payments.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Admin,Receptionist")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetAllAsync(cancellationToken);
        return FromResult(result, payments => Ok(payments));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, payment => Ok(payment));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreateAsync(request, cancellationToken);
        return FromResult(result, payment => CreatedAtAction(nameof(GetById), new { id = payment.PaymentId }, payment));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, payment => Ok(payment));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
