using Application.Common.Results;

namespace Application.Features.VehicleModels.Errors;

public static class VehicleModelErrors
{
    public static readonly Error NotFound = new("VehicleModels.NotFound", "Vehicle model was not found.");
    public static readonly Error BrandIdInvalid = new("VehicleModels.BrandIdInvalid", "BrandId must be greater than 0.");
    public static readonly Error BrandNotFound = new("VehicleModels.BrandNotFound", "Vehicle brand was not found.");
    public static readonly Error ModelNameRequired = new("VehicleModels.ModelNameRequired", "Model name is required.");
    public static readonly Error ModelNameTooLong = new("VehicleModels.ModelNameTooLong", "Model name cannot exceed 80 characters.");
    public static readonly Error ModelNameAlreadyExists = new("VehicleModels.ModelNameAlreadyExists", "Model name already exists for the selected brand.");
    public static readonly Error InUse = new("VehicleModels.InUse", "Vehicle model is assigned to one or more vehicles.");
}
