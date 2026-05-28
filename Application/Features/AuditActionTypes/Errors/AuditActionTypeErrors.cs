using Application.Common.Results;

namespace Application.Features.AuditActionTypes.Errors;

public static class AuditActionTypeErrors
{
    public static readonly Error NotFound = new("AuditActionTypes.NotFound", "Audit action type was not found.");
    public static readonly Error NameRequired = new("AuditActionTypes.NameRequired", "Audit action type name is required.");
    public static readonly Error NameTooLong = new("AuditActionTypes.NameTooLong", "Audit action type name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("AuditActionTypes.NameAlreadyExists", "Audit action type name already exists.");
    public static readonly Error InUse = new("AuditActionTypes.InUse", "Audit action type is assigned to one or more audits.");
}
