using Application.Features.VehicleBrands;
using Application.Features.VehicleBrands.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/vehicle-brands")]
public class VehicleBrandsController : BaseApiController
{
    private readonly IVehicleBrandService _vehicleBrandService;

    public VehicleBrandsController(IVehicleBrandService vehicleBrandService)
    {
        _vehicleBrandService = vehicleBrandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.GetAllAsync(cancellationToken);
        return FromResult(result, vehicleBrands => Ok(vehicleBrands));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, vehicleBrand => Ok(vehicleBrand));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.CreateAsync(request, cancellationToken);
        return FromResult(result, vehicleBrand => CreatedAtAction(nameof(GetById), new { id = vehicleBrand.BrandId }, vehicleBrand));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVehicleBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, vehicleBrand => Ok(vehicleBrand));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _vehicleBrandService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
