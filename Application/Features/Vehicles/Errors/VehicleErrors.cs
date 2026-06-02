using Application.Common.Results;

namespace Application.Features.Vehicles.Errors;

public static class VehicleErrors
{
    public static readonly Error NotFound = new("Vehicles.NotFound", "Vehicle was not found.");
    public static readonly Error ModelIdInvalid = new("Vehicles.ModelIdInvalid", "ModelId must be greater than 0.");
    public static readonly Error ModelNotFound = new("Vehicles.ModelNotFound", "Vehicle model was not found.");
    public static readonly Error VehicleTypeIdInvalid = new("Vehicles.VehicleTypeIdInvalid", "VehicleTypeId must be greater than 0.");
    public static readonly Error VehicleTypeNotFound = new("Vehicles.VehicleTypeNotFound", "Vehicle type was not found.");
    public static readonly Error VinRequired = new("Vehicles.VinRequired", "VIN is required.");
    public static readonly Error VinInvalid = new("Vehicles.VinInvalid", "VIN format is invalid.");
    public static readonly Error VinTooLong = new("Vehicles.VinTooLong", "VIN cannot exceed 17 characters.");
    public static readonly Error VinAlreadyExists = new("Vehicles.VinAlreadyExists", "VIN already exists.");
    public static readonly Error PlateRequired = new("Vehicles.PlateRequired", "Plate is required.");
    public static readonly Error PlateInvalid = new("Vehicles.PlateInvalid", "Plate is invalid.");
    public static readonly Error PlateAlreadyExists = new("Vehicles.PlateAlreadyExists", "Plate already exists for an active vehicle.");
    public static readonly Error YearInvalid = new("Vehicles.YearInvalid", "Year is invalid.");
    public static readonly Error ColorTooLong = new("Vehicles.ColorTooLong", "Color cannot exceed 30 characters.");
    public static readonly Error MileageInvalid = new("Vehicles.MileageInvalid", "Mileage must be greater than or equal to 0.");
    public static readonly Error InUse = new("Vehicles.InUse", "Vehicle is assigned to one or more records.");
}
