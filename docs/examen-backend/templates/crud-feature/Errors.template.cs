// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewEntities/Errors/NewEntityErrors.cs
// Referencia: Application/Features/Genders/Errors/GenderErrors.cs
//             Application/Features/Vehicles/Errors/VehicleErrors.cs
//
// IMPORTANTE: el sufijo del Code determina el HTTP en BaseApiController:
//   NotFound → 404 | Required/Invalid/TooLong → 400 | AlreadyExists/Conflict/InUse → 409

using Application.Common.Results;

namespace Application.Features.NewEntities.Errors;

public static class NewEntityErrors
{
    public static readonly Error NotFound = new("NewEntities.NotFound", "New entity was not found.");
    public static readonly Error NameRequired = new("NewEntities.NameRequired", "Name is required.");
    public static readonly Error NameTooLong = new("NewEntities.NameTooLong", "Name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("NewEntities.NameAlreadyExists", "Name already exists.");
    public static readonly Error InUse = new("NewEntities.InUse", "New entity is assigned to one or more records.");
}
