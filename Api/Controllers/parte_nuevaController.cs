using Application.Features.parte_nueva;
using Application.Features.parte_nueva.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/parte_nueva")]
[Authorize(Roles = "Admin")]
public class parte_nuevaController : BaseApiController
{
    private readonly Iparte_nuevaService _parte_nuevaService;

    public parte_nuevaController(Iparte_nuevaService parte_nuevaService)
    {
        _parte_nuevaService = parte_nuevaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _parte_nuevaService.GetAllAsync(cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _parte_nuevaService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, item => Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] Createparte_nuevaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _parte_nuevaService.CreateAsync(request, cancellationToken);
        return FromResult(result, item => CreatedAtAction(nameof(GetById), new { id = item.parte_nuevaId }, item));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] Updateparte_nuevaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _parte_nuevaService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, item => Ok(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _parte_nuevaService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
