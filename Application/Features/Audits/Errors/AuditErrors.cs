using Application.Common.Results;

namespace Application.Features.Audits.Errors;

public static class AuditErrors
{
    public static readonly Error NotFound = new("Audits.NotFound", "Audit was not found.");
    public static readonly Error UserIdInvalid = new("Audits.UserIdInvalid", "UserId must be greater than 0.");
    public static readonly Error UserNotFound = new("Audits.UserNotFound", "User was not found.");
    public static readonly Error AuditActionTypeIdInvalid = new("Audits.AuditActionTypeIdInvalid", "AuditActionTypeId must be greater than 0.");
    public static readonly Error AuditActionTypeNotFound = new("Audits.AuditActionTypeNotFound", "Audit action type was not found.");
    public static readonly Error AffectedEntityRequired = new("Audits.AffectedEntityRequired", "AffectedEntity is required.");
    public static readonly Error AffectedEntityTooLong = new("Audits.AffectedEntityTooLong", "AffectedEntity cannot exceed 100 characters.");
    public static readonly Error AffectedRecordIdInvalid = new("Audits.AffectedRecordIdInvalid", "AffectedRecordId must be greater than 0.");
}
