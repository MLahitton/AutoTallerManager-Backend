using Application.Common.Results;

namespace Application.Features.Countries.Errors;

public static class CountryErrors
{
    public static readonly Error NotFound = new("Countries.NotFound", "Country was not found.");
    public static readonly Error NameRequired = new("Countries.NameRequired", "Country name is required.");
    public static readonly Error NameTooLong = new("Countries.NameTooLong", "Country name cannot exceed 100 characters.");
    public static readonly Error PhoneCodeTooLong = new("Countries.PhoneCodeTooLong", "Country phone code cannot exceed 10 characters.");
    public static readonly Error NameAlreadyExists = new("Countries.NameAlreadyExists", "Country name already exists.");
    public static readonly Error InUse = new("Countries.InUse", "Country is assigned to one or more records.");
}
