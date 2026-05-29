using Application.Features.VehicleTypes;
using Application.Features.VehicleTypes.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/vehicle-types")]
[Authorize(Roles = "Admin")]
public class VehicleTypesController : BaseApiController
{
    private readonly IVehicleTypeService _vehicleTypeService;

    public VehicleTypesController(IVehicleTypeService vehicleTypeService)
    {
        _vehicleTypeService = vehicleTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _vehicleTypeService.GetAllAsync(cancellationToken);
        return FromResult(result, vehicleTypes => Ok(vehicleTypes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleTypeService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, vehicleType => Ok(vehicleType));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleTypeService.CreateAsync(request, cancellationToken);
        return FromResult(result, vehicleType => CreatedAtAction(nameof(GetById), new { id = vehicleType.VehicleTypeId }, vehicleType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVehicleTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleTypeService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, vehicleType => Ok(vehicleType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleTypeService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
