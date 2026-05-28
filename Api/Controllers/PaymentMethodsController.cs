using Application.Features.PaymentMethods;
using Application.Features.PaymentMethods.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController : BaseApiController
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.GetAllAsync(cancellationToken);
        return FromResult(result, paymentMethods => Ok(paymentMethods));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, paymentMethod => Ok(paymentMethod));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.CreateAsync(request, cancellationToken);
        return FromResult(result, paymentMethod => CreatedAtAction(nameof(GetById), new { id = paymentMethod.PaymentMethodId }, paymentMethod));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, paymentMethod => Ok(paymentMethod));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
