using Application.Common.Results;

namespace Application.Features.StreetTypes.Errors;

public static class StreetTypeErrors
{
    public static readonly Error NotFound = new("StreetTypes.NotFound", "Street type was not found.");
    public static readonly Error NameRequired = new("StreetTypes.NameRequired", "Street type name is required.");
    public static readonly Error NameTooLong = new("StreetTypes.NameTooLong", "Street type name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("StreetTypes.NameAlreadyExists", "Street type name already exists.");
    public static readonly Error InUse = new("StreetTypes.InUse", "Street type is assigned to one or more addresses.");
}
