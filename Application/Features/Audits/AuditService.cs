using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Audits.Dtos;
using Application.Features.Audits.Errors;
using Domain.Entities;

namespace Application.Features.Audits;

public class AuditService : IAuditService
{
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
}
