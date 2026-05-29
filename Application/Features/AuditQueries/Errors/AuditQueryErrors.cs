using Application.Common.Results;

namespace Application.Features.AuditQueries.Errors;

public static class AuditQueryErrors
{
    public static readonly Error EntityRequired = new("AuditQueries.EntityRequired", "Entity is required.");
    public static readonly Error RecordIdInvalid = new("AuditQueries.RecordIdInvalid", "RecordId must be greater than 0.");
    public static readonly Error UserIdInvalid = new("AuditQueries.UserIdInvalid", "UserId must be greater than 0.");
    public static readonly Error UserNotFound = new("AuditQueries.UserNotFound", "User was not found.");
}
