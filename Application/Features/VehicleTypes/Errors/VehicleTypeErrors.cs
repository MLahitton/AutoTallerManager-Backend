using Application.Common.Results;

namespace Application.Features.VehicleTypes.Errors;

public static class VehicleTypeErrors
{
    public static readonly Error NotFound = new("VehicleTypes.NotFound", "Vehicle type was not found.");
    public static readonly Error NameRequired = new("VehicleTypes.NameRequired", "Vehicle type name is required.");
    public static readonly Error NameTooLong = new("VehicleTypes.NameTooLong", "Vehicle type name cannot exceed 80 characters.");
    public static readonly Error NameAlreadyExists = new("VehicleTypes.NameAlreadyExists", "Vehicle type name already exists.");
    public static readonly Error InUse = new("VehicleTypes.InUse", "Vehicle type is assigned to one or more vehicles.");
}
