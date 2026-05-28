using Application.Common.Results;

namespace Application.Features.PersonRoles.Errors;

public static class PersonRoleErrors
{
    public static readonly Error NotFound = new("PersonRoles.NotFound", "Person role was not found.");
    public static readonly Error PersonIdInvalid = new("PersonRoles.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("PersonRoles.PersonNotFound", "Person was not found.");
    public static readonly Error RoleIdInvalid = new("PersonRoles.RoleIdInvalid", "RoleId must be greater than 0.");
    public static readonly Error RoleNotFound = new("PersonRoles.RoleNotFound", "Role was not found.");
    public static readonly Error RelationAlreadyExists = new("PersonRoles.RelationAlreadyExists", "Person role relation already exists.");
}
