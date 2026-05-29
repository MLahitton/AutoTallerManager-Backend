using Application.Features.Vehicles;
using Application.Features.Vehicles.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize(Roles = "Admin,Receptionist")]
public class VehiclesController : BaseApiController
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _vehicleService.GetAllAsync(cancellationToken);
        return FromResult(result, vehicles => Ok(vehicles));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, vehicle => Ok(vehicle));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleService.CreateAsync(request, cancellationToken);
        return FromResult(result, vehicle => CreatedAtAction(nameof(GetById), new { id = vehicle.VehicleId }, vehicle));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, vehicle => Ok(vehicle));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
