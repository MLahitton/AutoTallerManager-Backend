// PLANTILLA DE ESTUDIO — NO COMPILAR
// Fragmentos para agregar en Application/Features/{Entidad}s/{Entidad}Service.cs
// Referencia completa: Application/Features/Vehicles/VehicleService.cs (Plate)

// --- 1. Constantes al inicio de la clase ---
// private const int NewFieldMaxLength = 50;
// private static readonly Regex NewFieldPattern = new(@"^...$", RegexOptions.CultureInvariant);

// --- 2. En CreateAsync: leer y normalizar ---
// var newField = NormalizeNewField(request?.NewField);

// --- 3. Incluir en Validate(...) ---
// private static Error? Validate(..., string newField)
// {
//     if (string.IsNullOrWhiteSpace(newField))
//         return VehicleErrors.NewFieldRequired;
//     if (newField.Length > NewFieldMaxLength)
//         return VehicleErrors.NewFieldTooLong;
//     if (!NewFieldPattern.IsMatch(newField))
//         return VehicleErrors.NewFieldInvalid;
//     return null;
// }

// --- 4. Unicidad (si aplica) ---
// var exists = await vehicleRepository.ExistsAsync(
//     x => x.NewField == newField && x.IsActive,
//     cancellationToken);
// if (exists)
//     return Result<VehicleDto>.Failure(VehicleErrors.NewFieldAlreadyExists);

// --- 5. En UpdateAsync: excluir id actual en unicidad ---
// x => x.NewField == newField && x.VehicleId != id

// --- 6. Asignar a la entidad ---
// vehicle.NewField = newField;

// --- 7. Agregar errores en Errors/{Entidad}Errors.cs ---
// public static readonly Error NewFieldRequired = new("Vehicles.NewFieldRequired", "...");
// public static readonly Error NewFieldAlreadyExists = new("Vehicles.NewFieldAlreadyExists", "...");

// --- Métodos helper (patrón VehicleService) ---
// private static string NormalizeNewField(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
