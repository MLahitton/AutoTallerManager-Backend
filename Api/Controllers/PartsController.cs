using Application.Features.Parts;
using Application.Features.Parts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize(Roles = "Admin,Receptionist")]
public class PartsController : BaseApiController
{
    private readonly IPartService _partService;

    public PartsController(IPartService partService)
    {
        _partService = partService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _partService.GetAllAsync(cancellationToken);
        return FromResult(result, parts => Ok(parts));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _partService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, part => Ok(part));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartRequest request, CancellationToken cancellationToken)
    {
        var result = await _partService.CreateAsync(request, cancellationToken);
        return FromResult(result, part => CreatedAtAction(nameof(GetById), new { id = part.PartId }, part));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartRequest request, CancellationToken cancellationToken)
    {
        var result = await _partService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, part => Ok(part));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _partService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
