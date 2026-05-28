using Application.Common.Results;

namespace Application.Features.Departments.Errors;

public static class DepartmentErrors
{
    public static readonly Error NotFound = new("Departments.NotFound", "Department was not found.");
    public static readonly Error CountryNotFound = new("Departments.CountryNotFound", "Country was not found.");
    public static readonly Error CountryIdInvalid = new("Departments.CountryIdInvalid", "CountryId must be greater than 0.");
    public static readonly Error NameRequired = new("Departments.NameRequired", "Department name is required.");
    public static readonly Error NameTooLong = new("Departments.NameTooLong", "Department name cannot exceed 100 characters.");
    public static readonly Error NameAlreadyExists = new("Departments.NameAlreadyExists", "Department name already exists in the selected country.");
    public static readonly Error InUse = new("Departments.InUse", "Department is assigned to one or more cities.");
}
