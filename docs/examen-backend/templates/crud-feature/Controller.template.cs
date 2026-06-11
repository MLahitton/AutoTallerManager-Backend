// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Api/Controllers/NewEntitiesController.cs
// Referencia: Api/Controllers/GendersController.cs
//             Api/Controllers/VehiclesController.cs

using Application.Features.NewEntities;
using Application.Features.NewEntities.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/new-entities")]  // Ajustar ruta según el enunciado (kebab-case plural)
[Authorize(Roles = "Admin,Receptionist")]  // Ajustar roles según el caso
public class NewEntitiesController : BaseApiController
{
    private readonly INewEntityService _newEntityService;

    public NewEntitiesController(INewEntityService newEntityService)
    {
        _newEntityService = newEntityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _newEntityService.GetAllAsync(cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _newEntityService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, item => Ok(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateNewEntityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _newEntityService.CreateAsync(request, cancellationToken);
        return FromResult(
            result,
            item => CreatedAtAction(nameof(GetById), new { id = item.NewEntityId }, item));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateNewEntityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _newEntityService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, item => Ok(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _newEntityService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    // Si el CRUD requiere auditoría, extrae userId del token como en SuppliersController:
    // private bool TryGetCurrentUserId(out int userId) { ... User.FindFirstValue("userId") ... }
}
