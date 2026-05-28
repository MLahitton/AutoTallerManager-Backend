using Application.Common.Results;

namespace Application.Features.PersonEmails.Errors;

public static class PersonEmailErrors
{
    public static readonly Error NotFound = new("PersonEmails.NotFound", "Person email was not found.");
    public static readonly Error PersonIdInvalid = new("PersonEmails.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("PersonEmails.PersonNotFound", "Person was not found.");
    public static readonly Error EmailDomainIdInvalid = new("PersonEmails.EmailDomainIdInvalid", "EmailDomainId must be greater than 0.");
    public static readonly Error EmailDomainNotFound = new("PersonEmails.EmailDomainNotFound", "Email domain was not found.");
    public static readonly Error EmailUserRequired = new("PersonEmails.EmailUserRequired", "Email user is required.");
    public static readonly Error EmailUserTooLong = new("PersonEmails.EmailUserTooLong", "Email user cannot exceed 100 characters.");
    public static readonly Error EmailUserInvalid = new("PersonEmails.EmailUserInvalid", "Email user format is invalid.");
    public static readonly Error EmailAlreadyExists = new("PersonEmails.EmailAlreadyExists", "Email already exists.");
    public static readonly Error PrimaryAlreadyExists = new("PersonEmails.PrimaryAlreadyExists", "A primary email already exists for this person.");
}
