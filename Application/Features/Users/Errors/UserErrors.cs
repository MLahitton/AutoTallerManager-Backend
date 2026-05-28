using Application.Common.Results;

namespace Application.Features.Users.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new("Users.NotFound", "User was not found.");
    public static readonly Error PersonIdInvalid = new("Users.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("Users.PersonNotFound", "Person was not found.");
    public static readonly Error PersonAlreadyExists = new("Users.PersonAlreadyExists", "Person already has a user.");
    public static readonly Error PasswordRequired = new("Users.PasswordRequired", "Password is required.");
    public static readonly Error PasswordTooShort = new("Users.PasswordTooShort", "Password must be at least 8 characters long.");
    public static readonly Error PasswordTooLong = new("Users.PasswordTooLong", "Password cannot exceed 100 characters.");
    public static readonly Error InUse = new("Users.InUse", "User is assigned to one or more records.");
}
