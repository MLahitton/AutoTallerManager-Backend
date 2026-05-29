using Application.Features.CardTypes;
using Application.Features.CardTypes.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/card-types")]
[Authorize(Roles = "Admin")]
public class CardTypesController : BaseApiController
{
    private readonly ICardTypeService _cardTypeService;

    public CardTypesController(ICardTypeService cardTypeService)
    {
        _cardTypeService = cardTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _cardTypeService.GetAllAsync(cancellationToken);
        return FromResult(result, cardTypes => Ok(cardTypes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _cardTypeService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, cardType => Ok(cardType));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCardTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cardTypeService.CreateAsync(request, cancellationToken);
        return FromResult(result, cardType => CreatedAtAction(nameof(GetById), new { id = cardType.CardTypeId }, cardType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCardTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cardTypeService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, cardType => Ok(cardType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _cardTypeService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
