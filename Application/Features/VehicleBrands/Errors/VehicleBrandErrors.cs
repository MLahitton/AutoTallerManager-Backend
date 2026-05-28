using Application.Common.Results;

namespace Application.Features.VehicleBrands.Errors;

public static class VehicleBrandErrors
{
    public static readonly Error NotFound = new("VehicleBrands.NotFound", "Vehicle brand was not found.");
    public static readonly Error BrandNameRequired = new("VehicleBrands.BrandNameRequired", "Vehicle brand name is required.");
    public static readonly Error BrandNameTooLong = new("VehicleBrands.BrandNameTooLong", "Vehicle brand name cannot exceed 80 characters.");
    public static readonly Error BrandNameAlreadyExists = new("VehicleBrands.BrandNameAlreadyExists", "Vehicle brand name already exists.");
    public static readonly Error InUse = new("VehicleBrands.InUse", "Vehicle brand is assigned to one or more vehicle models.");
}
