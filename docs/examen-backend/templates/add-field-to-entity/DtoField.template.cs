// PLANTILLA DE ESTUDIO — NO COMPILAR
// Modificar: Application/Features/{Entidad}s/Dtos/{Entidad}Dto.cs
// Referencia: Application/Features/Vehicles/Dtos/VehicleDto.cs — campo Plate

namespace Application.Features.Vehicles.Dtos;

public class VehicleDto
{
    public int VehicleId { get; set; }
    // ... campos existentes ...

    // Agregar el mismo campo que en la entidad (mismo nombre lógico)
    // public string NewField { get; set; } = string.Empty;
}

// También actualizar MapToDto en {Entidad}Service.cs:
// NewField = vehicle.NewField,
