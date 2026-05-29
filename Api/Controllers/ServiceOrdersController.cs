using Application.Features.ServiceOrders;
using Application.Features.ServiceOrders.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        var result = await _serviceOrderService.CreateAsync(request, cancellationToken);
        return FromResult(result, serviceOrder => CreatedAtAction(nameof(GetById), new { id = serviceOrder.ServiceOrderId }, serviceOrder));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, serviceOrder => Ok(serviceOrder));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _serviceOrderService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
