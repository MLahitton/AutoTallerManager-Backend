using Application.Features.VehicleModels;
using Application.Features.VehicleModels.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/vehicle-models")]
[Authorize(Roles = "Admin")]
public class VehicleModelsController : BaseApiController
{
    private readonly IVehicleModelService _vehicleModelService;

    public VehicleModelsController(IVehicleModelService vehicleModelService)
    {
        _vehicleModelService = vehicleModelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.GetAllAsync(cancellationToken);
        return FromResult(result, vehicleModels => Ok(vehicleModels));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, vehicleModel => Ok(vehicleModel));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleModelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.CreateAsync(request, cancellationToken);
        return FromResult(result, vehicleModel => CreatedAtAction(nameof(GetById), new { id = vehicleModel.ModelId }, vehicleModel));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVehicleModelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, vehicleModel => Ok(vehicleModel));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleModelService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
