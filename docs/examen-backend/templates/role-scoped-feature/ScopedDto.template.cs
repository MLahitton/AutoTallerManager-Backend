// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewFeature/Dtos/NewScopedDto.cs
// Referencia: Application/Features/ClientApprovals/Dtos/ClientPendingApprovalDto.cs
//             Application/Features/ServiceExecution/Dtos/MechanicAssignedServiceDto.cs
//             Application/Features/AdminMechanics/Dtos/AdminMechanicDto.cs

namespace Application.Features.NewFeature.Dtos;

// DTO para listados scoped — incluir solo campos que el rol puede ver.
public class NewScopedDto
{
    public int NewEntityId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Campos de contexto útiles para el frontend:
    // public int ServiceOrderId { get; set; }
    // public string VehiclePlate { get; set; }
    // public string Status { get; set; }
}
