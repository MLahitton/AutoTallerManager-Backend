using Application.Common.Results;

namespace Application.Features.Genders.Errors;

public static class GenderErrors
{
    public static readonly Error NotFound = new("Genders.NotFound", "Gender was not found.");
    public static readonly Error NameRequired = new("Genders.NameRequired", "Gender name is required.");
    public static readonly Error NameTooLong = new("Genders.NameTooLong", "Gender name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("Genders.NameAlreadyExists", "Gender name already exists.");
    public static readonly Error InUse = new("Genders.InUse", "Gender is assigned to one or more persons.");
}
