using Application.Common.Results;

namespace Application.Features.ServiceTypes.Errors;

public static class ServiceTypeErrors
{
    public static readonly Error NotFound = new("ServiceTypes.NotFound", "Service type was not found.");
    public static readonly Error NameRequired = new("ServiceTypes.NameRequired", "Service type name is required.");
    public static readonly Error NameTooLong = new("ServiceTypes.NameTooLong", "Service type name cannot exceed 100 characters.");
    public static readonly Error NameAlreadyExists = new("ServiceTypes.NameAlreadyExists", "Service type name already exists.");
    public static readonly Error EstimatedDaysInvalid = new("ServiceTypes.EstimatedDaysInvalid", "Estimated days must be greater than or equal to 1.");
    public static readonly Error InUse = new("ServiceTypes.InUse", "Service type is assigned to one or more order services.");
}
