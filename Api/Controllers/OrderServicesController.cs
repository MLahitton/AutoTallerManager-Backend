using Application.Features.OrderServices;
using Application.Features.OrderServices.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/order-services")]
[Authorize(Roles = "Admin,Receptionist")]
public class OrderServicesController : BaseApiController
{
    private readonly IOrderServiceService _orderServiceService;

    public OrderServicesController(IOrderServiceService orderServiceService)
    {
        _orderServiceService = orderServiceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _orderServiceService.GetAllAsync(cancellationToken);
        return FromResult(result, orderServices => Ok(orderServices));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _orderServiceService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, orderService => Ok(orderService));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderServiceService.CreateAsync(request, cancellationToken);
        return FromResult(result, orderService => CreatedAtAction(nameof(GetById), new { id = orderService.OrderServiceId }, orderService));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOrderServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderServiceService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, orderService => Ok(orderService));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _orderServiceService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
