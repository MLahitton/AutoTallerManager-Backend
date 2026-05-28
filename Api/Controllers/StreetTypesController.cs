using Application.Features.StreetTypes;
using Application.Features.StreetTypes.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/street-types")]
public class StreetTypesController : BaseApiController
{
    private readonly IStreetTypeService _streetTypeService;

    public StreetTypesController(IStreetTypeService streetTypeService)
    {
        _streetTypeService = streetTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _streetTypeService.GetAllAsync(cancellationToken);
        return FromResult(result, streetTypes => Ok(streetTypes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _streetTypeService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, streetType => Ok(streetType));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStreetTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _streetTypeService.CreateAsync(request, cancellationToken);
        return FromResult(result, streetType => CreatedAtAction(nameof(GetById), new { id = streetType.StreetTypeId }, streetType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateStreetTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _streetTypeService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, streetType => Ok(streetType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _streetTypeService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
