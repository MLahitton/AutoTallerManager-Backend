using Application.Features.VehicleOwnerHistories;
using Application.Features.VehicleOwnerHistories.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/vehicle-owner-histories")]
[Authorize(Roles = "Admin,Receptionist")]
public class VehicleOwnerHistoriesController : BaseApiController
{
    private readonly IVehicleOwnerHistoryService _vehicleOwnerHistoryService;

    public VehicleOwnerHistoriesController(IVehicleOwnerHistoryService vehicleOwnerHistoryService)
    {
        _vehicleOwnerHistoryService = vehicleOwnerHistoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _vehicleOwnerHistoryService.GetAllAsync(cancellationToken);
        return FromResult(result, vehicleOwnerHistories => Ok(vehicleOwnerHistories));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleOwnerHistoryService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, vehicleOwnerHistory => Ok(vehicleOwnerHistory));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleOwnerHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleOwnerHistoryService.CreateAsync(request, cancellationToken);
        return FromResult(result, vehicleOwnerHistory => CreatedAtAction(nameof(GetById), new { id = vehicleOwnerHistory.VehicleOwnerHistoryId }, vehicleOwnerHistory));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVehicleOwnerHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleOwnerHistoryService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, vehicleOwnerHistory => Ok(vehicleOwnerHistory));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleOwnerHistoryService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
