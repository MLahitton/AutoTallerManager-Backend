using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.AuditQueries.Dtos;
using Application.Features.AuditQueries.Errors;
using Domain.Entities;

namespace Application.Features.AuditQueries;

public class AuditQueryService : IAuditQueryService
{
    private const int RecentLimit = 50;
    private readonly IUnitOfWork _unitOfWork;

    public AuditQueryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AuditQueryDto>>> GetRecentAsync(CancellationToken cancellationToken = default)
    {
        var audits = await _unitOfWork.Repository<Audit>().GetAllAsync(cancellationToken);

        var result = audits
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.AuditId)
            .Take(RecentLimit)
            .Select(MapAudit)
            .ToList();

        return Result<IReadOnlyList<AuditQueryDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<AuditQueryDto>>> GetByEntityAsync(
        string? entity,
        int recordId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEntity = NormalizeOptionalText(entity);
        if (string.IsNullOrWhiteSpace(normalizedEntity))
        {
            return Result<IReadOnlyList<AuditQueryDto>>.Failure(AuditQueryErrors.EntityRequired);
        }

        if (recordId <= 0)
        {
            return Result<IReadOnlyList<AuditQueryDto>>.Failure(AuditQueryErrors.RecordIdInvalid);
        }

        var auditsWithRecord = await _unitOfWork.Repository<Audit>().FindAsync(
            x => x.AffectedRecordId == recordId,
            cancellationToken);

        var result = auditsWithRecord
            .Where(x => x.AffectedEntity.Equals(normalizedEntity, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.AuditId)
            .Select(MapAudit)
            .ToList();

        return Result<IReadOnlyList<AuditQueryDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<AuditQueryDto>>> GetByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<IReadOnlyList<AuditQueryDto>>.Failure(AuditQueryErrors.UserIdInvalid);
        }

        var userExists = await _unitOfWork.Repository<User>().ExistsAsync(
            x => x.UserId == userId,
            cancellationToken);

        if (!userExists)
        {
            return Result<IReadOnlyList<AuditQueryDto>>.Failure(AuditQueryErrors.UserNotFound);
        }

        var audits = await _unitOfWork.Repository<Audit>().FindAsync(
            x => x.UserId == userId,
            cancellationToken);

        var result = audits
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.AuditId)
            .Select(MapAudit)
            .ToList();

        return Result<IReadOnlyList<AuditQueryDto>>.Success(result);
    }

    private static AuditQueryDto MapAudit(Audit audit)
    {
        return new AuditQueryDto
        {
            AuditId = audit.AuditId,
            UserId = audit.UserId,
            AuditActionTypeId = audit.AuditActionTypeId,
            AffectedEntity = audit.AffectedEntity,
            AffectedRecordId = audit.AffectedRecordId,
            Description = audit.Description,
            CreatedAt = audit.CreatedAt
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
