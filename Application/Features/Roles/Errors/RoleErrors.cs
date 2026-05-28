using Application.Common.Results;

namespace Application.Features.Roles.Errors;

public static class RoleErrors
{
    public static readonly Error NotFound = new("Roles.NotFound", "Role was not found.");
    public static readonly Error NameRequired = new("Roles.NameRequired", "Role name is required.");
    public static readonly Error NameTooLong = new("Roles.NameTooLong", "Role name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("Roles.NameAlreadyExists", "Role name already exists.");
    public static readonly Error RoleInUse = new("Roles.RoleInUse", "Role is assigned to one or more persons.");
}
