// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewEntities/Dtos/NewEntityDto.cs
// Referencia: Application/Features/Genders/Dtos/GenderDto.cs

namespace Application.Features.NewEntities.Dtos;

// DTO de salida: expone lo que el API devuelve al cliente.
public class NewEntityDto
{
    public int NewEntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
