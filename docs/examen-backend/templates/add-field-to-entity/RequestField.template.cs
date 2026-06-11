// PLANTILLA DE ESTUDIO — NO COMPILAR
// Modificar:
//   Application/Features/{Entidad}s/Requests/Create{Entidad}Request.cs
//   Application/Features/{Entidad}s/Requests/Update{Entidad}Request.cs
// Referencia: Application/Features/Vehicles/Requests/CreateVehicleRequest.cs

namespace Application.Features.Vehicles.Requests;

public class CreateVehicleRequest
{
    // ... propiedades existentes ...

    // Campo nuevo — nullable en create si la validación está en el servicio
    // public string? NewField { get; set; }
}

public class UpdateVehicleRequest
{
    // Incluir el campo también en update
    // public string? NewField { get; set; }
}
