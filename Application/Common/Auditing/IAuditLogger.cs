namespace Application.Common.Auditing;

public interface IAuditLogger
{
    Task LogAsync(
        int userId,
        string actionTypeName,
        string affectedEntity,
        int affectedRecordId,
        string? description,
        CancellationToken cancellationToken = default);
}
