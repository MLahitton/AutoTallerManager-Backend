using Application.Common.Results;

namespace Application.Features.Neighborhoods.Errors;

public static class NeighborhoodErrors
{
    public static readonly Error NotFound = new("Neighborhoods.NotFound", "Neighborhood was not found.");
    public static readonly Error CityNotFound = new("Neighborhoods.CityNotFound", "City was not found.");
    public static readonly Error CityIdInvalid = new("Neighborhoods.CityIdInvalid", "CityId must be greater than 0.");
    public static readonly Error NameRequired = new("Neighborhoods.NameRequired", "Neighborhood name is required.");
    public static readonly Error NameTooLong = new("Neighborhoods.NameTooLong", "Neighborhood name cannot exceed 100 characters.");
    public static readonly Error NameAlreadyExists = new("Neighborhoods.NameAlreadyExists", "Neighborhood name already exists in the selected city.");
    public static readonly Error InUse = new("Neighborhoods.InUse", "Neighborhood is assigned to one or more addresses.");
}
