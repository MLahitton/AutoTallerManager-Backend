// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewEntities/Requests/UpdateNewEntityRequest.cs
// Referencia: Application/Features/Genders/Requests/UpdateGenderRequest.cs

namespace Application.Features.NewEntities.Requests;

public class UpdateNewEntityRequest
{
    public string? Name { get; set; }
    public bool IsActive { get; set; }
}
