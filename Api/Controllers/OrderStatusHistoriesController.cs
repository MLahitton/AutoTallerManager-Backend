using Application.Features.OrderStatusHistories;
using Application.Features.OrderStatusHistories.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/order-status-histories")]
[Authorize(Roles = "Admin,Receptionist")]
public class OrderStatusHistoriesController : BaseApiController
{
    private readonly IOrderStatusHistoryService _orderStatusHistoryService;

    public OrderStatusHistoriesController(IOrderStatusHistoryService orderStatusHistoryService)
    {
        _orderStatusHistoryService = orderStatusHistoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _orderStatusHistoryService.GetAllAsync(cancellationToken);
        return FromResult(result, orderStatusHistories => Ok(orderStatusHistories));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _orderStatusHistoryService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, orderStatusHistory => Ok(orderStatusHistory));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderStatusHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderStatusHistoryService.CreateAsync(request, cancellationToken);
        return FromResult(result, orderStatusHistory => CreatedAtAction(nameof(GetById), new { id = orderStatusHistory.OrderStatusHistoryId }, orderStatusHistory));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOrderStatusHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderStatusHistoryService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, orderStatusHistory => Ok(orderStatusHistory));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _orderStatusHistoryService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
