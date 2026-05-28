using Application.Features.ServiceTypes;
using Application.Features.ServiceTypes.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/service-types")]
public class ServiceTypesController : BaseApiController
{
    private readonly IServiceTypeService _serviceTypeService;

    public ServiceTypesController(IServiceTypeService serviceTypeService)
    {
        _serviceTypeService = serviceTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.GetAllAsync(cancellationToken);
        return FromResult(result, serviceTypes => Ok(serviceTypes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, serviceType => Ok(serviceType));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.CreateAsync(request, cancellationToken);
        return FromResult(result, serviceType => CreatedAtAction(nameof(GetById), new { id = serviceType.ServiceTypeId }, serviceType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateServiceTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, serviceType => Ok(serviceType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _serviceTypeService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
