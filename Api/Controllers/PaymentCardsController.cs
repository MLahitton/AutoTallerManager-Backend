using Application.Features.PaymentCards;
using Application.Features.PaymentCards.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/payment-cards")]
[Authorize(Roles = "Admin,Receptionist")]
public class PaymentCardsController : BaseApiController
{
    private readonly IPaymentCardService _paymentCardService;

    public PaymentCardsController(IPaymentCardService paymentCardService)
    {
        _paymentCardService = paymentCardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentCardService.GetAllAsync(cancellationToken);
        return FromResult(result, paymentCards => Ok(paymentCards));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentCardService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, paymentCard => Ok(paymentCard));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCardRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentCardService.CreateAsync(request, cancellationToken);
        return FromResult(result, paymentCard => CreatedAtAction(nameof(GetById), new { id = paymentCard.PaymentCardId }, paymentCard));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentCardRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentCardService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, paymentCard => Ok(paymentCard));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentCardService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
