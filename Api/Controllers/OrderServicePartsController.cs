using Application.Features.OrderServiceParts;
using Application.Features.OrderServiceParts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/order-service-parts")]
[Authorize(Roles = "Admin,Receptionist")]
public class OrderServicePartsController : BaseApiController
{
    private readonly IOrderServicePartService _orderServicePartService;

    public OrderServicePartsController(IOrderServicePartService orderServicePartService)
    {
        _orderServicePartService = orderServicePartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _orderServicePartService.GetAllAsync(cancellationToken);
        return FromResult(result, orderServiceParts => Ok(orderServiceParts));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _orderServicePartService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, orderServicePart => Ok(orderServicePart));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderServicePartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderServicePartService.CreateAsync(request, cancellationToken);
        return FromResult(result, orderServicePart => CreatedAtAction(nameof(GetById), new { id = orderServicePart.OrderServicePartId }, orderServicePart));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOrderServicePartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderServicePartService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, orderServicePart => Ok(orderServicePart));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _orderServicePartService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
