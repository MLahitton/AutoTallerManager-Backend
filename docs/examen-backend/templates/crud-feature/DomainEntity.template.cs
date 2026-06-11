// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Domain/Entities/NewEntity.cs
// Referencia: Domain/Entities/Gender.cs (simple) o Domain/Entities/Vehicle.cs (con relaciones)

namespace Domain.Entities;

// Entidad de dominio pura: sin atributos EF, sin validación DataAnnotations.
public class NewEntity
{
    // Clave primaria — convención del proyecto: {Entidad}Id
    public int NewEntityId { get; set; }

    // Propiedades escalares del negocio
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Si hay relación con otra entidad, agregar FK + navegación:
    // public int OtherEntityId { get; set; }
    // public OtherEntity Other { get; set; } = null!;
    // public ICollection<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}
