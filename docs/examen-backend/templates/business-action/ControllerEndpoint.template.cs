// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Api/Controllers/NewFeatureBusinessController.cs (o agregar método a controller existente)
// Referencia: Api/Controllers/InventoryBusinessController.cs
//             Api/Controllers/InvoiceBusinessController.cs
//             Api/Controllers/PaymentBusinessController.cs
//             Api/Controllers/ClientApprovalsController.cs

using System.Security.Claims;
using Application.Features.NewFeature;
using Application.Features.NewFeature.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/new-feature")]  // Ruta semántica, no necesariamente el nombre de la entidad
public class NewFeatureBusinessController : BaseApiController
{
    private readonly INewFeatureBusinessService _newFeatureBusinessService;

    public NewFeatureBusinessController(INewFeatureBusinessService newFeatureBusinessService)
    {
        _newFeatureBusinessService = newFeatureBusinessService;
    }

    // Acción de negocio: verbo + sustantivo en la ruta
    [HttpPost("entities/{newEntityId:int}/execute-action")]
    [Authorize(Roles = "Admin,Receptionist")]  // Ajustar según enunciado
    public async Task<IActionResult> ExecuteAction(
        int newEntityId,
        [FromBody] ExecuteNewFeatureActionRequest request,
        CancellationToken cancellationToken)
    {
        // NUNCA tomar userId del body — siempre del JWT
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _newFeatureBusinessService.ExecuteActionAsync(
            newEntityId,
            request,
            currentUserId,
            cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdClaim = User.FindFirstValue("userId");
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}

// Ejemplos de rutas reales en el proyecto:
// POST api/inventory/purchases/{purchaseId}/cancel
// POST api/client/order-services/{orderServiceId}/approve
// POST api/invoices/from-service-order/{serviceOrderId}  (ver InvoiceBusinessController)
