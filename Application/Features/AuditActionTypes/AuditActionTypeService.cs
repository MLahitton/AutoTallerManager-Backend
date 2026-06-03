using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.AuditActionTypes.Dtos;
using Application.Features.AuditActionTypes.Errors;
using Domain.Entities;

namespace Application.Features.AuditActionTypes;

public class AuditActionTypeService : IAuditActionTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditActionTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AuditActionTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionTypes = await auditActionTypeRepository.GetAllAsync(cancellationToken);

        var auditActionTypeDtos = auditActionTypes
            .OrderBy(x => x.AuditActionTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<AuditActionTypeDto>>.Success(auditActionTypeDtos);
    }

    public async Task<Result<AuditActionTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionType = await auditActionTypeRepository.GetByIdAsync(id, cancellationToken);

        if (auditActionType is null)
        {
            return Result<AuditActionTypeDto>.Failure(AuditActionTypeErrors.NotFound);
        }

        return Result<AuditActionTypeDto>.Success(MapToDto(auditActionType));
    }

    private static AuditActionTypeDto MapToDto(AuditActionType auditActionType)
    {
        return new AuditActionTypeDto
        {
            AuditActionTypeId = auditActionType.AuditActionTypeId,
            Name = auditActionType.Name
        };
    }
}
