using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Audits.Dtos;
using Application.Features.Audits.Errors;
using Application.Features.Audits.Requests;
using Domain.Entities;

namespace Application.Features.Audits;

public class AuditService : IAuditService
{
    private const int AffectedEntityMaxLength = 100;

    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AuditDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var auditRepository = _unitOfWork.Repository<Audit>();
        var audits = await auditRepository.GetAllAsync(cancellationToken);

        var auditDtos = audits
            .OrderBy(x => x.AuditId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<AuditDto>>.Success(auditDtos);
    }

    public async Task<Result<AuditDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var auditRepository = _unitOfWork.Repository<Audit>();
        var audit = await auditRepository.GetByIdAsync(id, cancellationToken);

        if (audit is null)
        {
            return Result<AuditDto>.Failure(AuditErrors.NotFound);
        }

        return Result<AuditDto>.Success(MapToDto(audit));
    }

    public async Task<Result<AuditDto>> CreateAsync(CreateAuditRequest request, CancellationToken cancellationToken = default)
    {
        var userId = request?.UserId ?? 0;
        var auditActionTypeId = request?.AuditActionTypeId ?? 0;
        var affectedEntity = NormalizeRequiredText(request?.AffectedEntity);
        var affectedRecordId = request?.AffectedRecordId ?? 0;
        var description = NormalizeOptionalText(request?.Description);

        var validationError = Validate(userId, auditActionTypeId, affectedEntity, affectedRecordId);
        if (validationError is not null)
        {
            return Result<AuditDto>.Failure(validationError);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var userExists = await userRepository.ExistsAsync(
            x => x.UserId == userId,
            cancellationToken);

        if (!userExists)
        {
            return Result<AuditDto>.Failure(AuditErrors.UserNotFound);
        }

        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionTypeExists = await auditActionTypeRepository.ExistsAsync(
            x => x.AuditActionTypeId == auditActionTypeId,
            cancellationToken);

        if (!auditActionTypeExists)
        {
            return Result<AuditDto>.Failure(AuditErrors.AuditActionTypeNotFound);
        }

        var audit = new Audit
        {
            UserId = userId,
            AuditActionTypeId = auditActionTypeId,
            AffectedEntity = affectedEntity,
            AffectedRecordId = affectedRecordId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        var auditRepository = _unitOfWork.Repository<Audit>();
        await auditRepository.AddAsync(audit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuditDto>.Success(MapToDto(audit));
    }

    public async Task<Result<AuditDto>> UpdateAsync(int id, UpdateAuditRequest request, CancellationToken cancellationToken = default)
    {
        var auditRepository = _unitOfWork.Repository<Audit>();
        var audit = await auditRepository.GetByIdAsync(id, cancellationToken);

        if (audit is null)
        {
            return Result<AuditDto>.Failure(AuditErrors.NotFound);
        }

        var userId = request?.UserId ?? 0;
        var auditActionTypeId = request?.AuditActionTypeId ?? 0;
        var affectedEntity = NormalizeRequiredText(request?.AffectedEntity);
        var affectedRecordId = request?.AffectedRecordId ?? 0;
        var description = NormalizeOptionalText(request?.Description);

        var validationError = Validate(userId, auditActionTypeId, affectedEntity, affectedRecordId);
        if (validationError is not null)
        {
            return Result<AuditDto>.Failure(validationError);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var userExists = await userRepository.ExistsAsync(
            x => x.UserId == userId,
            cancellationToken);

        if (!userExists)
        {
            return Result<AuditDto>.Failure(AuditErrors.UserNotFound);
        }

        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionTypeExists = await auditActionTypeRepository.ExistsAsync(
            x => x.AuditActionTypeId == auditActionTypeId,
            cancellationToken);

        if (!auditActionTypeExists)
        {
            return Result<AuditDto>.Failure(AuditErrors.AuditActionTypeNotFound);
        }

        audit.UserId = userId;
        audit.AuditActionTypeId = auditActionTypeId;
        audit.AffectedEntity = affectedEntity;
        audit.AffectedRecordId = affectedRecordId;
        audit.Description = description;

        auditRepository.Update(audit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuditDto>.Success(MapToDto(audit));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var auditRepository = _unitOfWork.Repository<Audit>();
        var audit = await auditRepository.GetByIdAsync(id, cancellationToken);

        if (audit is null)
        {
            return Result.Failure(AuditErrors.NotFound);
        }

        auditRepository.Remove(audit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static AuditDto MapToDto(Audit audit)
    {
        return new AuditDto
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

    private static Error? Validate(
        int userId,
        int auditActionTypeId,
        string affectedEntity,
        int affectedRecordId)
    {
        if (userId <= 0)
        {
            return AuditErrors.UserIdInvalid;
        }

        if (auditActionTypeId <= 0)
        {
            return AuditErrors.AuditActionTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(affectedEntity))
        {
            return AuditErrors.AffectedEntityRequired;
        }

        if (affectedEntity.Length > AffectedEntityMaxLength)
        {
            return AuditErrors.AffectedEntityTooLong;
        }

        if (affectedRecordId <= 0)
        {
            return AuditErrors.AffectedRecordIdInvalid;
        }

        return null;
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
