using System.Security.Claims;
using Application.Features.ServiceOrders;
using Application.Features.ServiceOrders.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/service-orders")]
[Authorize(Roles = "Admin,Receptionist")]
public class ServiceOrdersController : BaseApiController
{
    private readonly IServiceOrderService _serviceOrderService;

    public ServiceOrdersController(IServiceOrderService serviceOrderService)
    {
        _serviceOrderService = serviceOrderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.GetAllAsync(cancellationToken);
        return FromResult(result, serviceOrders => Ok(serviceOrders));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, serviceOrder => Ok(serviceOrder));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, serviceOrder => CreatedAtAction(nameof(GetById), new { id = serviceOrder.ServiceOrderId }, serviceOrder));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, serviceOrder => Ok(serviceOrder));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _serviceOrderService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
