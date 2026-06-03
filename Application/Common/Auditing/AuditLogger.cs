using Application.Common.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Common.Auditing;

public class AuditLogger : IAuditLogger
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogger(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(
        int userId,
        string actionTypeName,
        string affectedEntity,
        int affectedRecordId,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var normalizedActionTypeName = NormalizeRequiredText(actionTypeName);
        var normalizedAffectedEntity = NormalizeRequiredText(affectedEntity);

        if (userId <= 0 ||
            string.IsNullOrWhiteSpace(normalizedActionTypeName) ||
            string.IsNullOrWhiteSpace(normalizedAffectedEntity) ||
            affectedRecordId <= 0)
        {
            return;
        }

        var auditActionTypes = await _unitOfWork.Repository<AuditActionType>().GetAllAsync(cancellationToken);
        var actionType = auditActionTypes.FirstOrDefault(x =>
            x.Name.Equals(normalizedActionTypeName, StringComparison.OrdinalIgnoreCase));

        if (actionType is null)
        {
            return;
        }

        var audit = new Audit
        {
            UserId = userId,
            AuditActionTypeId = actionType.AuditActionTypeId,
            AffectedEntity = normalizedAffectedEntity,
            AffectedRecordId = affectedRecordId,
            Description = NormalizeOptionalText(description),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Audit>().AddAsync(audit, cancellationToken);
    }

    private static string NormalizeRequiredText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
