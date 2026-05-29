using Application.Features.OrderStatuses;
using Application.Features.OrderStatuses.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/order-statuses")]
[Authorize(Roles = "Admin")]
public class OrderStatusesController : BaseApiController
{
    private readonly IOrderStatusService _orderStatusService;

    public OrderStatusesController(IOrderStatusService orderStatusService)
    {
        _orderStatusService = orderStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _orderStatusService.GetAllAsync(cancellationToken);
        return FromResult(result, orderStatuses => Ok(orderStatuses));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _orderStatusService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, orderStatus => Ok(orderStatus));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderStatusService.CreateAsync(request, cancellationToken);
        return FromResult(result, orderStatus => CreatedAtAction(nameof(GetById), new { id = orderStatus.OrderStatusId }, orderStatus));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderStatusService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, orderStatus => Ok(orderStatus));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _orderStatusService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
