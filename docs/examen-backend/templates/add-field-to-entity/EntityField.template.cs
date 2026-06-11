// PLANTILLA DE ESTUDIO — NO COMPILAR
// Modificar archivo existente: Domain/Entities/{Entidad}.cs
// Referencia real: Domain/Entities/Vehicle.cs — propiedad Plate

namespace Domain.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }
    // ... propiedades existentes ...

    // NUEVO CAMPO — agregar junto a las demás propiedades escalares
    // public string NewField { get; set; } = string.Empty;
    // public DateTime? OptionalDate { get; set; }
    // public int? OptionalForeignKeyId { get; set; }
}
