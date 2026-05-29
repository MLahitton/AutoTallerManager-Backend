using Application.Features.Neighborhoods;
using Application.Features.Neighborhoods.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/neighborhoods")]
[Authorize(Roles = "Admin")]
public class NeighborhoodsController : BaseApiController
{
    private readonly INeighborhoodService _neighborhoodService;

    public NeighborhoodsController(INeighborhoodService neighborhoodService)
    {
        _neighborhoodService = neighborhoodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _neighborhoodService.GetAllAsync(cancellationToken);
        return FromResult(result, neighborhoods => Ok(neighborhoods));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _neighborhoodService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, neighborhood => Ok(neighborhood));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateNeighborhoodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _neighborhoodService.CreateAsync(request, cancellationToken);
        return FromResult(result, neighborhood => CreatedAtAction(nameof(GetById), new { id = neighborhood.NeighborhoodId }, neighborhood));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateNeighborhoodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _neighborhoodService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, neighborhood => Ok(neighborhood));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _neighborhoodService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
