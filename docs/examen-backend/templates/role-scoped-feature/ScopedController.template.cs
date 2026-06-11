// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Api/Controllers/NewScopedController.cs
// Referencia Client: Api/Controllers/ClientVehiclesController.cs, ClientApprovalsController.cs
// Referencia Mechanic: Api/Controllers/MechanicWorkflowController.cs
// Referencia Admin: Api/Controllers/AdminMechanicsController.cs

using System.Security.Claims;
using Application.Features.NewFeature;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/client")]  // o api/mechanic, api/admin/...
[Authorize(Roles = "Client")]  // Cambiar a Mechanic, Admin según el caso
public class NewScopedController : BaseApiController
{
    private readonly INewScopedService _newScopedService;

    public NewScopedController(INewScopedService newScopedService)
    {
        _newScopedService = newScopedService;
    }

    // Endpoint "mis datos" — personId SOLO del token
    [HttpGet("my-new-entities")]
    public async Task<IActionResult> GetMyNewEntities(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _newScopedService.GetMyNewEntitiesAsync(currentPersonId, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    // Acción que requiere userId (auditoría) y personId (ownership)
    [HttpPost("new-entities/{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentContext(out var currentUserId, out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _newScopedService.ConfirmAsync(id, currentPersonId, currentUserId, cancellationToken);
        return FromResult(result, dto => Ok(dto));
    }

    // --- Extracción de claims (NO confiar en el body) ---

    private bool TryGetCurrentPersonId(out int personId)
    {
        personId = 0;
        var personIdClaim = User.FindFirstValue("personId");
        return int.TryParse(personIdClaim, out personId) && personId > 0;
    }

    private bool TryGetCurrentContext(out int userId, out int personId)
    {
        userId = 0;
        personId = 0;

        var userIdClaim = User.FindFirstValue("userId");
        if (!int.TryParse(userIdClaim, out userId) || userId <= 0)
        {
            return false;
        }

        var personIdClaim = User.FindFirstValue("personId");
        return int.TryParse(personIdClaim, out personId) && personId > 0;
    }

    // Para mecánico, a veces también necesitas roles:
    // private bool TryGetCurrentContext(out int userId, out int personId, out IReadOnlyList<string> roles)
    // Ver MechanicWorkflowController.cs
}
