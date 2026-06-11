// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewEntities/Requests/CreateNewEntityRequest.cs
// Referencia: Application/Features/Genders/Requests/CreateGenderRequest.cs
//             Application/Features/Vehicles/Requests/CreateVehicleRequest.cs

namespace Application.Features.NewEntities.Requests;

// El request solo transporta datos. La validación va en el servicio.
public class CreateNewEntityRequest
{
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;

    // Campos opcionales suelen ser nullable (string?) para distinguir "no enviado"
}
