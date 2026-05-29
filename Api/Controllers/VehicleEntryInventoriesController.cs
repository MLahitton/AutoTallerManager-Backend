using Application.Features.VehicleEntryInventories;
using Application.Features.VehicleEntryInventories.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/vehicle-entry-inventories")]
public class VehicleEntryInventoriesController : BaseApiController
{
    private readonly IVehicleEntryInventoryService _vehicleEntryInventoryService;

    public VehicleEntryInventoriesController(IVehicleEntryInventoryService vehicleEntryInventoryService)
    {
        _vehicleEntryInventoryService = vehicleEntryInventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _vehicleEntryInventoryService.GetAllAsync(cancellationToken);
        return FromResult(result, vehicleEntryInventories => Ok(vehicleEntryInventories));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleEntryInventoryService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, vehicleEntryInventory => Ok(vehicleEntryInventory));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleEntryInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleEntryInventoryService.CreateAsync(request, cancellationToken);
        return FromResult(result, vehicleEntryInventory => CreatedAtAction(nameof(GetById), new { id = vehicleEntryInventory.EntryInventoryId }, vehicleEntryInventory));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVehicleEntryInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleEntryInventoryService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, vehicleEntryInventory => Ok(vehicleEntryInventory));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleEntryInventoryService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
