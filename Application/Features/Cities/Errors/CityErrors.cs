using Application.Common.Results;

namespace Application.Features.Cities.Errors;

public static class CityErrors
{
    public static readonly Error NotFound = new("Cities.NotFound", "City was not found.");
    public static readonly Error DepartmentNotFound = new("Cities.DepartmentNotFound", "Department was not found.");
    public static readonly Error DepartmentIdInvalid = new("Cities.DepartmentIdInvalid", "DepartmentId must be greater than 0.");
    public static readonly Error NameRequired = new("Cities.NameRequired", "City name is required.");
    public static readonly Error NameTooLong = new("Cities.NameTooLong", "City name cannot exceed 100 characters.");
    public static readonly Error NameAlreadyExists = new("Cities.NameAlreadyExists", "City name already exists in the selected department.");
    public static readonly Error InUse = new("Cities.InUse", "City is assigned to one or more neighborhoods.");
}
