using Application.Features.PartBrands;
using Application.Features.PartBrands.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/part-brands")]
[Authorize(Roles = "Admin")]
public class PartBrandsController : BaseApiController
{
    private readonly IPartBrandService _partBrandService;

    public PartBrandsController(IPartBrandService partBrandService)
    {
        _partBrandService = partBrandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _partBrandService.GetAllAsync(cancellationToken);
        return FromResult(result, partBrands => Ok(partBrands));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _partBrandService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, partBrand => Ok(partBrand));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partBrandService.CreateAsync(request, cancellationToken);
        return FromResult(result, partBrand => CreatedAtAction(nameof(GetById), new { id = partBrand.PartBrandId }, partBrand));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePartBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partBrandService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, partBrand => Ok(partBrand));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _partBrandService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
