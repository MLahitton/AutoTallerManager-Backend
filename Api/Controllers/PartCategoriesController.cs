using Application.Features.PartCategories;
using Application.Features.PartCategories.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/part-categories")]
public class PartCategoriesController : BaseApiController
{
    private readonly IPartCategoryService _partCategoryService;

    public PartCategoriesController(IPartCategoryService partCategoryService)
    {
        _partCategoryService = partCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _partCategoryService.GetAllAsync(cancellationToken);
        return FromResult(result, partCategories => Ok(partCategories));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _partCategoryService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, partCategory => Ok(partCategory));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partCategoryService.CreateAsync(request, cancellationToken);
        return FromResult(result, partCategory => CreatedAtAction(nameof(GetById), new { id = partCategory.PartCategoryId }, partCategory));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePartCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partCategoryService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, partCategory => Ok(partCategory));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _partCategoryService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
