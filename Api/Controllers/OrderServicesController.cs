using System.Security.Claims;
using Application.Features.OrderServices;
using Application.Features.OrderServices.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _orderServiceService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, orderService => CreatedAtAction(nameof(GetById), new { id = orderService.OrderServiceId }, orderService));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOrderServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _orderServiceService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, orderService => Ok(orderService));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _orderServiceService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
