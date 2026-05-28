using Application.Common.Results;

namespace Application.Features.EmailDomains.Errors;

public static class EmailDomainErrors
{
    public static readonly Error NotFound = new("EmailDomains.NotFound", "Email domain was not found.");
    public static readonly Error DomainRequired = new("EmailDomains.DomainRequired", "Email domain is required.");
    public static readonly Error DomainTooLong = new("EmailDomains.DomainTooLong", "Email domain cannot exceed 100 characters.");
    public static readonly Error DomainInvalid = new("EmailDomains.DomainInvalid", "Email domain format is invalid.");
    public static readonly Error DomainAlreadyExists = new("EmailDomains.DomainAlreadyExists", "Email domain already exists.");
    public static readonly Error InUse = new("EmailDomains.InUse", "Email domain is assigned to one or more person emails.");
}
