using Application.Features.MechanicSpecialties;
using Application.Features.MechanicSpecialties.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/mechanic-specialties")]
public class MechanicSpecialtiesController : BaseApiController
{
    private readonly IMechanicSpecialtyService _mechanicSpecialtyService;

    public MechanicSpecialtiesController(IMechanicSpecialtyService mechanicSpecialtyService)
    {
        _mechanicSpecialtyService = mechanicSpecialtyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyService.GetAllAsync(cancellationToken);
        return FromResult(result, mechanicSpecialties => Ok(mechanicSpecialties));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, mechanicSpecialty => Ok(mechanicSpecialty));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMechanicSpecialtyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyService.CreateAsync(request, cancellationToken);
        return FromResult(result, mechanicSpecialty => CreatedAtAction(nameof(GetById), new { id = mechanicSpecialty.SpecialtyId }, mechanicSpecialty));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMechanicSpecialtyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, mechanicSpecialty => Ok(mechanicSpecialty));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
