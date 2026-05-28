using Application.Features.Genders;
using Application.Features.Genders.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/genders")]
public class GendersController : BaseApiController
{
    private readonly IGenderService _genderService;

    public GendersController(IGenderService genderService)
    {
        _genderService = genderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _genderService.GetAllAsync(cancellationToken);
        return FromResult(result, genders => Ok(genders));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _genderService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, gender => Ok(gender));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGenderRequest request, CancellationToken cancellationToken)
    {
        var result = await _genderService.CreateAsync(request, cancellationToken);
        return FromResult(result, gender => CreatedAtAction(nameof(GetById), new { id = gender.GenderId }, gender));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateGenderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _genderService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, gender => Ok(gender));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _genderService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
